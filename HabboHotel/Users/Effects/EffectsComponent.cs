﻿using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;

namespace Plus.HabboHotel.Users.Effects
{
    public sealed class EffectsComponent
    {
        private Habbo _habbo;
        private int _currentEffect;

        /// <summary>
        /// Effects stored by ID > Effect.
        /// </summary>
        private readonly ConcurrentDictionary<int, AvatarEffect> _effects = new ConcurrentDictionary<int, AvatarEffect>();

        public EffectsComponent()
        {
        }

        /// <summary>
        /// Initializes the EffectsComponent.
        /// </summary>
        public bool Init(Habbo habbo)
        {
            if (_effects.Count > 0)
                return false;
            
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `user_effects` WHERE `user_id` = @id;");
                dbClient.AddParameter("id", habbo.Id);
                var getEffects = dbClient.GetTable();

                if (getEffects != null)
                {
                    foreach (DataRow row in getEffects.Rows)
                    {
                        if (_effects.TryAdd(Convert.ToInt32(row["id"]), new AvatarEffect(Convert.ToInt32(row["id"]), Convert.ToInt32(row["user_id"]), Convert.ToInt32(row["effect_id"]), Convert.ToDouble(row["total_duration"]), PlusEnvironment.EnumToBool(row["is_activated"].ToString()), Convert.ToDouble(row["activated_stamp"]), Convert.ToInt32(row["quantity"]))))
                        {
                            //umm?
                        }
                    }
                }
            }

            _habbo = habbo;
            _currentEffect = 0;
            return true;
        }

        public bool TryAdd(AvatarEffect effect)
        {
            return _effects.TryAdd(effect.Id, effect);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteId"></param>
        /// <param name="activatedOnly"></param>
        /// <param name="unactivatedOnly"></param>
        /// <returns></returns>
        public bool HasEffect(int spriteId, bool activatedOnly = false, bool unactivatedOnly = false)
        {
            return (GetEffectNullable(spriteId, activatedOnly, unactivatedOnly) != null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteId"></param>
        /// <param name="activatedOnly"></param>
        /// <param name="unactivatedOnly"></param>
        /// <returns></returns>
        public AvatarEffect GetEffectNullable(int spriteId, bool activatedOnly = false, bool unactivatedOnly = false)
        {
            foreach (var effect in _effects.Values.ToList())
            {
                if (!effect.HasExpired && effect.SpriteId == spriteId && (!activatedOnly || effect.Activated) && (!unactivatedOnly || !effect.Activated))
                {
                    return effect;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="habbo"></param>
        public void CheckEffectExpiry(Habbo habbo)
        {
            foreach (var effect in _effects.Values.ToList())
            {
                if (effect.HasExpired)
                {
                    effect.HandleExpiration(habbo);
                }
            }
        }

        public void ApplyEffect(int effectId)
        {
            if (_habbo == null || _habbo.CurrentRoom == null)
                return;

            var user = _habbo.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(_habbo.Id);
            if (user == null)
                return;

            _currentEffect = effectId;

            if (user.IsDancing)
                _habbo.CurrentRoom.SendPacket(new DanceComposer(user, 0));
            _habbo.CurrentRoom.SendPacket(new AvatarEffectComposer(user.VirtualId, effectId));
        }

        public ICollection<AvatarEffect> GetAllEffects
        {
            get { return _effects.Values; }
        }

        public int CurrentEffect
        {
            get { return _currentEffect; }
            set { _currentEffect = value; }
        }

        /// <summary>
        /// Disposes the EffectsComponent.
        /// </summary>
        public void Dispose()
        {
            _effects.Clear();
        }
    }
}
