using Dapper;
using Microsoft.Extensions.Logging;
using Plus.Database;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.Users.Clothing.Parts;
using Plus.Utilities.FigureData;
using Plus.Utilities.FigureData.Types;
using System.Data;
using Z.Dapper.Plus;

namespace Plus.HabboHotel.Users.Clothing;
internal class FigureDataManager : IFigureDataManager
{
    private readonly ICatalogManager _catalogManager;
    private readonly ILogger<FigureDataManager> _logger;
    private readonly IFigureDataUtility _figureDataUtility;
    private readonly IDatabase _database;

    private const bool DEBUG = true;
    private const int MaxItemComponents = 5; //Should always be 2 + highest colorindex of parts of the item.//TODO make dynamic
    private const string Male = "M";
    private const string Female = "F";
    private const string Unisex = "U";
    private readonly string[] mandatorySetTypes = { "ch", "hd", "lg" };
    private readonly string[] setTypes = { "ca", "cc", "ch", "cp", "ea", "fa", "ha", "hd", "he", "hr", "lg", "sh", "wa" };


    public FigureDataManager(ICatalogManager catalogManager, ILogger<FigureDataManager> logger, IFigureDataUtility figureDataUtility, IDatabase database)
    {
        _catalogManager = catalogManager;
        _logger = logger;
        _figureDataUtility = figureDataUtility;
        _database = database;
    }

    public async Task InitAsync()
    {
        FigureData figureData = await _figureDataUtility.InitFromJsonAsync();
        ConfigureDapperPlusMappings();
        await IngestDataToDatabaseAsync(figureData);
    }

    public async Task UpdateFigureData() => await _figureDataUtility.UpdateFromJsonAsync();

    private void LogMessage(string message)
    {
        if (DEBUG)
        {
            Console.WriteLine(message);
        }
    }
    public void ConfigureDapperPlusMappings()
    {
        DapperPlusManager.Entity<Palette>()
        .Table("figure_palettes")  // Assuming this is the table name for Palette
        .Identity(x => x.Id)
        .AfterAction((kind, x) => {
            if (kind == DapperPlusActionKind.Insert || kind == DapperPlusActionKind.Merge)
            {
                x.Colors.ForEach(y => y.PaletteId = x.Id);
            }
        });

        // Mapping for figure_palettes
        DapperPlusManager.Entity<Color>()
            .Table("figure_palettes")
            .Identity(x => x.primaryId, "id")
            .Map(x => x.Id, "color_id")
            .Map(x => x.PaletteId, "palette_id")
            .Map(x => x.Club);

        // Mapping for figure_sets
        DapperPlusManager.Entity<Set>()
            .Table("figure_sets")
            .Identity(x => x.Id)
            .Map(x => x.Id, "id")
            .Map(x => x.SetTypeReference, "set_type")
            .Map(x => x.Gender, "gender")
            .Map(x => x.Club, "club")
            .Map(x => x.Sellable, "sellable")
            .Map(x => x.Colorable, "colorable")
            .Map(x => x.Colors, "colors");

        // Mapping for figure_types
        DapperPlusManager.Entity<SetType>()
           .Table("figure_types")
           .Map(x => x.Type, "type")
           .Map(x => x.PaletteId, "paletteId");
    }

    private async Task IngestDataToDatabaseAsync(FigureData figureData)
    {
        using var connection = _database.Connection();

        var setTypeDataList = new List<SetType>();
        var setDataList = new List<Set>();
        var paletteDataList = new List<Color>();

        foreach (var setType in figureData.SetTypes)
        {
            setTypeDataList.Add(setType);

            foreach (var set in setType.Sets)
            {
                set.SetTypeReference = setType.Type;
                set.Colors = set.Parts.Max(part => part.ColorIndex);
                setDataList.Add(set);
            }
        }

        foreach (var palette in figureData.Palettes)
        {
            paletteDataList.AddRange(palette.Colors);

            foreach(var palettes in palette.Colors)
            {
                palettes.PaletteId = palette.Id;
                paletteDataList.Add(palettes);
            }
        }
        try
        {
            await connection.BulkActionAsync(x => x.BulkMerge(setTypeDataList));
            await connection.BulkActionAsync(x => x.BulkMerge(setDataList));
            await connection.BulkActionAsync(x => x.BulkMerge(paletteDataList));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while ingesting data to the database: {ex.Message}");
        }
    }

    private async Task<bool> ValidateColorAsync(long colorIndex, int paletteId, bool hasHabboClub, IDbConnection connection)
    {
        const string sql = @"
        SELECT 
            COUNT(*) AS PaletteCount, 
            SUM(CASE WHEN color_id = @ColorIndex THEN 1 ELSE 0 END) AS ColorCount,
            MAX(CASE WHEN color_id = @ColorIndex THEN club ELSE 0 END) AS ClubRequirement
        FROM figure_palettes
        WHERE palette_id = @PaletteId
    ";

        var parameters = new { ColorIndex = colorIndex, PaletteId = paletteId };

        var result = await connection.QueryFirstOrDefaultAsync<(int PaletteCount, int ColorCount, int ClubRequirement)>(sql, parameters);

        if (result.PaletteCount == 0)
        {
            LogMessage($"Failed: palette not found for palette ID '{paletteId}'");
            return false;
        }

        if (result.ColorCount == 0)
        {
            LogMessage($"Failed: color index '{colorIndex}' not found in palette ID '{paletteId}'");
            return false;
        }

        if (result.ClubRequirement > 0 && !hasHabboClub)
        {
            LogMessage($"Failed: color index '{colorIndex}' requires Habbo Club membership");
            return false;
        }

        return true;
    }
    private async Task<string> HandleColorableSetTypeAsync(bool hasHabboClub, string[] splitItem, IDbConnection connection)
    {
        var itemType = splitItem[0];
        var setId = splitItem[1];
        var paletteId = await GetPaletteIdForTypeAsync(itemType, connection);
        var validatedColors = new List<string>();  // Hold validated colors
        int? firstValidColorId = null;

        for (var i = 2; i < splitItem.Length; i++)
        {
            var colorId = int.Parse(splitItem[i]);
            bool colorIsValid = await ValidateColorAsync(colorId, paletteId, hasHabboClub, connection);

            if (colorIsValid)
            {
                validatedColors.Add(colorId.ToString());  // Add the valid color to the list
                continue;
            }

            firstValidColorId ??= await GetFirstValidColorAsync(paletteId, connection);

            if (firstValidColorId.HasValue)
            {
                validatedColors.Add(firstValidColorId.Value.ToString());  // Add the first valid color to the list
            }
        }

        return string.Join("-", new[] { itemType, setId }.Concat(validatedColors));
    }
    private async Task<int?> GetFirstValidColorAsync(int paletteId, IDbConnection connection)
    {
        string query = @"SELECT color_id
                     FROM figure_palettes
                     WHERE palette_id = @PaletteId
                     AND club = 0
                     ORDER BY id ASC
                     LIMIT 1";

        var firstValidColorId = await connection.QueryFirstOrDefaultAsync<int?>(query, new { PaletteId = paletteId });

        if (firstValidColorId == null)
        {
            LogMessage($"Failed: No valid color found for paletteId '{paletteId}'");
        }
        else
        {
            LogMessage($"Success: First valid color for paletteId '{paletteId}' is '{firstValidColorId}'");
        }

        return firstValidColorId;
    }
    private async Task<string> ValidateSingleItemAsync(string item, string gender, bool hasHabboClub, IDbConnection connection)
    {
        var splitItem = item.Split('-');
        var itemType = splitItem[0];

        if (splitItem.Length > MaxItemComponents)
        {
            LogMessage($"Warning: Too many colors specified for item '{item}'. Truncating to {MaxItemComponents} components.");
            splitItem = splitItem.Take(MaxItemComponents).ToArray();
        }

        const string sql = @"SELECT gender, club, colorable 
                          FROM figure_sets 
                          WHERE id = @Id AND set_type = @Type";

        var parameters = new { Id = int.Parse(splitItem[1]), Type = itemType };
        var setItem = await connection.QueryFirstOrDefaultAsync(sql, parameters);

        if (setItem is null || (!hasHabboClub && setItem.club == 1) || (setItem.gender != Unisex && setItem.gender != gender))
        {
            return await GenerateSetByTypeAsync(itemType, connection);
        }

        if (setItem.colorable)
        {
            return await HandleColorableSetTypeAsync(hasHabboClub, splitItem, connection);
        }
        else
        {
            return $"{itemType}-{splitItem[1]}";
        }

    }
    private async Task<int> GetPaletteIdForTypeAsync(string setType, IDbConnection connection)
    {
        const string query = @"
        SELECT paletteId 
        FROM figure_types
        WHERE type = @SetType
        LIMIT 1";

        var parameters = new { SetType = setType };

        var paletteId = await connection.QueryFirstOrDefaultAsync<int>(query, parameters);

        return paletteId;
    }

    private async Task<string> GenerateSetByTypeAsync(string setType, IDbConnection connection)
    {
        const string query = @"
        SELECT id 
        FROM figure_sets
        WHERE set_type = @SetType AND
              gender = @Gender AND
              club = 0 AND
              colorable = 1 AND
              colors = 1
        LIMIT 1";

        var parameters = new
        {
            SetType = setType,
            Gender = Unisex
        };

        var setId = await connection.QueryFirstOrDefaultAsync<int>(query, parameters);
        var palette = await GetPaletteIdForTypeAsync(setType, connection);
        var color = await GetFirstValidColorAsync(palette, connection);

        return $"{setType}-{setId}-{color}";
    }

    //TODO add ICollection<ClothingParts> clothingParts
    public async Task<string> ValidateLookAsync(string look, string gender, ICollection<ClothingParts> clothingParts, bool hasHabboClub)
    {
        LogMessage(look);
        var items = look.Split('.');
        var validatedItems = new List<string>();
        var existingSetTypes = new HashSet<string>();

        using var connection = _database.Connection();

        if (string.IsNullOrEmpty(look) ||
            (gender != Male && gender != Female))
        {
            // return GenerateDefaultLook(); TODO
            return "hd-180-1.lg-999999906-79";
        }

        foreach (var item in items)
        {
            var parts = item.Split('-');
            var setType = parts[0];
            var isValidItem = true;  // flag to indicate if the entire item is valid

            if (!setTypes.Contains(setType))
            {
                LogMessage($"Failed: setType '{setType}' was invalid and removed.");
                isValidItem = false;
            }
    
            if (existingSetTypes.Contains(setType))
            {
                LogMessage($"Failed: Duplicate setType '{setType}' was removed.");
                isValidItem = false;
            }
    
            if (isValidItem)
            {
                // Check that the parts after the setType are positive integers
                for (var i = 1; i < parts.Length; i++)
                {
                    if (!int.TryParse(parts[i], out var value) || value < 0)
                    {
                        LogMessage($"Failed: Invalid integer '{parts[i]}' in item '{item}'.");
                        isValidItem = false;
                        break;
                    }
                }

                // If all checks pass, proceed to ValidateSingleItemAsync
                if (isValidItem)
                {
                    var validatedItem = await ValidateSingleItemAsync(item, gender, hasHabboClub, connection);
                    if (string.IsNullOrEmpty(validatedItem))
                    {
                        var split = item.Split('-');
                        var type = split[0];

                        validatedItems.Add(await GenerateSetByTypeAsync(type, connection));
                    }
                    validatedItems.Add(validatedItem);
                }
            }
        }
        var finalItems = new HashSet<string>(
            validatedItems.Select(item => item.Split('-')[0])
        );

        /*After the validation we now check if the look contains all the Mandatory types 
        and if not we create one.
        We dont need to call validateSingleItem anymore as we make sure to create valid ones.*/

         foreach (var setType in mandatorySetTypes)
         {
             if (!finalItems.Contains(setType))
             {
                validatedItems.Add(await GenerateSetByTypeAsync(setType, connection));
             }
        }

        LogMessage($"All Validated Items: {string.Join(", ", validatedItems)}");

        return string.Join(".", validatedItems);
    }

}