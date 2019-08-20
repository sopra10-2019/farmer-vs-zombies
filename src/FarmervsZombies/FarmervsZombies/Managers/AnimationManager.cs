using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.Managers
{
    /// <summary>
    /// Manages all Animations in the game.
    ///
    /// How to use an existing Animation in the AnimationManager (example):
    /// 1. Define the Animation:
    ///     mWalkDownAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerWalkDown, this);
    /// 2. Overwrite the Texture of the GameObject in the Update function:
    ///     mTexture = mWalkDownAnimation.GetTexture()
    ///
    /// How to use a not already existing Animation in the AnimationManager (example):
    /// 1. Add the Animation to the AnimationManager:
    ///     mWalkDownAnimation = AnimationManager.GetAnimation(AnimationManager.AddAnimation, this);
    /// 2. Overwrite the Texture of the GameObject in the Update function:
    ///     mTexture = mWalkDownAnimation.GetTexture()
    /// </summary>
    internal static class AnimationManager
    {
        // All animations that are managed by the AnimationManager
        private static readonly List<Animation> sAnimations = new List<Animation>();
        private static readonly Dictionary<object, List<Animation>> sAnimationUsers = new Dictionary<object, List<Animation>>();


        public static Animation FarmerWalkUp =>
            new Animation(TextureManager.GetTexture("farmer_walk", 128, 64, 0), 32, 64, 0, -1, 0.1f);

        public static Animation FarmerWalkRight => new Animation(TextureManager.GetTexture("farmer_walk", 128, 64, 3), 32, 64, 0, -1, 0.1f);


        public static Animation FarmerWalkDown => new Animation(TextureManager.GetTexture("farmer_walk", 128, 64, 2), 32, 64, 0, -1, 0.1f);

        public static Animation FarmerWalkLeft => new Animation(TextureManager.GetTexture("farmer_walk", 128, 64, 1), 32, 64, 0, -1, 0.1f);

        public static Animation FarmerStandUp => new Animation(TextureManager.GetTexture("farmer_stand", 128, 64, 0), 32, 64, 0, -1, 0.2f);

        public static Animation FarmerStandRight => new Animation(TextureManager.GetTexture("farmer_stand", 128, 64, 1), 32, 64, 0, -1, 0.2f);

        public static Animation FarmerStandDown => new Animation(TextureManager.GetTexture("farmer_stand", 128, 64, 2), 32, 64, 0, -1, 0.2f);

        public static Animation FarmerStandLeft => new Animation(TextureManager.GetTexture("farmer_stand", 128, 64, 3), 32, 64, 0, -1, 0.2f);

        public static Animation FarmerAttack1Up => new Animation(TextureManager.GetTexture("farmer_attack1", 128, 64, 0), 32, 64, 0, -1, 0.1f);

        public static Animation FarmerAttack1Right => new Animation(TextureManager.GetTexture("farmer_attack1", 128, 64, 1), 32, 64, 0, -1, 0.1f);

        public static Animation FarmerAttack1Down => new Animation(TextureManager.GetTexture("farmer_attack1", 128, 64, 2), 32, 64, 0, -1, 0.1f);

        public static Animation FarmerAttack1Left => new Animation(TextureManager.GetTexture("farmer_attack1", 128, 64, 3), 32, 64, 0, -1, 0.1f);

        public static Animation FarmerAttack2Up => new Animation(TextureManager.GetTexture("farmer_attack2", 256, 64, 0), 64, 64, -0.5f, -1, 0.1f);

        public static Animation FarmerAttack2Right => new Animation(TextureManager.GetTexture("farmer_attack2", 256, 64, 3), 64, 64, -0.5f, -1, 0.1f);

        public static Animation FarmerAttack2Down => new Animation(TextureManager.GetTexture("farmer_attack2", 256, 64, 2), 64, 64, -0.5f, -1, 0.1f);

        public static Animation FarmerAttack2Left => new Animation(TextureManager.GetTexture("farmer_attack2", 256, 64, 1), 64, 64, -0.5f, -1, 0.1f);

        public static Animation ChickenWalkUp => new Animation(TextureManager.GetTexture("chicken_walk", 128, 32, 0), 32, 32, 0, 0, 0.1f);

        public static Animation ChickenWalkLeft => new Animation(TextureManager.GetTexture("chicken_walk", 128, 32, 1), 32, 32, 0, 0, 0.1f);

        public static Animation ChickenWalkDown => new Animation(TextureManager.GetTexture("chicken_walk", 128, 32, 2), 32, 32, 0, 0, 0.1f);

        public static Animation ChickenWalkRight => new Animation(TextureManager.GetTexture("chicken_walk", 128, 32, 3), 32, 32, 0, 0, 0.1f);

        public static Animation PigWalkUp => new Animation(TextureManager.GetTexture("pig_walk", 128, 64, 0), 32, 64, 0, -0.5f, 0.1f);

        public static Animation PigWalkLeft => new Animation(TextureManager.GetTexture("pig_walk", 128, 64, 1), 64, 32, -0.5f, 0, 0.1f);

        public static Animation PigWalkDown => new Animation(TextureManager.GetTexture("pig_walk", 128, 64, 2), 32, 64, 0, -0.5f, 0.1f);

        public static Animation PigWalkRight => new Animation(TextureManager.GetTexture("pig_walk", 128, 64, 3), 64, 32, -0.5f, 0, 0.1f);

        public static Animation CowWalkUp => new Animation(TextureManager.GetTexture("cow_walk", 128, 64, 0), 32, 64, 0, -0.5f, 0.1f);

        public static Animation CowWalkLeft => new Animation(TextureManager.GetTexture("cow_walk", 128, 64, 1), 64, 32, -0.5f, 0, 0.1f);

        public static Animation CowWalkDown => new Animation(TextureManager.GetTexture("cow_walk", 128, 64, 2), 32, 64, 0, -0.5f, 0.1f);

        public static Animation CowWalkRight => new Animation(TextureManager.GetTexture("cow_walk", 128, 64, 3), 64, 32, -0.5f, 0, 0.1f);

        public static Animation ZombieStandRight => new Animation(TextureManager.GetTexture("zombie_idle", 192, 64, 0), 32, 64, 0, -1, 0.1f);

        public static Animation ZombieStandLeft => new Animation(TextureManager.GetTexture("zombie_idle", 192, 64, 1), 32, 64, 0, -1, 0.1f);

        public static Animation ZombieWalkRight => new Animation(TextureManager.GetTexture("zombie_walk", 192, 64, 1), 32, 64, 0, -1, 0.1f);

        public static Animation ZombieWalkLeft => new Animation(TextureManager.GetTexture("zombie_walk", 192, 64, 0), 32, 64, 0, -1, 0.1f);

        public static Animation ZombieDeadRight => new Animation(TextureManager.GetTexture("zombie_dead", 512, 64, 1), 64, 64, 0, -1, 0.1f);

        public static Animation ZombieDeadLeft => new Animation(TextureManager.GetTexture("zombie_dead", 512, 64, 0), 64, 64, -1, -1, 0.1f);

        public static Animation ZombieAttackLeft => new Animation(TextureManager.GetTexture("zombie_attack", 192, 64, 0), 32, 64, 0, -1, 0.05f);

        public static Animation ZombieAttackRight => new Animation(TextureManager.GetTexture("zombie_attack", 192, 64, 1), 32, 64, 0, -1, 0.05f);

        public static Animation NecromancerWalkDown => new Animation(TextureManager.GetTexture("necromancer_big_walk", 96, 64, 0), 32, 64, 0, -1, 0.1f);

        public static Animation NecromancerWalkUp => new Animation(TextureManager.GetTexture("necromancer_big_walk", 96, 64, 1), 32, 64, 0, -1, 0.1f);

        public static Animation NecromancerWalkRight => new Animation(TextureManager.GetTexture("necromancer_big_walk", 96, 64, 2), 32, 64, 0, -1, 0.1f);

        public static Animation NecromancerWalkLeft => new Animation(TextureManager.GetTexture("necromancer_big_walk", 96, 64, 3), 32, 64, 0, -1, 0.1f);

        public static Animation ChickenEatLeft => new Animation(TextureManager.GetTexture("chicken_eat", 128, 32, 0), 32, 32, 0, 0, 0.18f);

        public static Animation ChickenEatRight => new Animation(TextureManager.GetTexture("chicken_eat", 128, 32, 3), 32, 32, 0, 0, 0.18f);

        public static Animation ChickenEatUp => new Animation(TextureManager.GetTexture("chicken_eat", 128, 32, 0), 32, 32, 0, 0, 0.18f);

        public static Animation ChickenEatDown => new Animation(TextureManager.GetTexture("chicken_eat", 128, 32, 2), 32, 32, 0, 0, 0.18f);

        public static Animation CowEatLeft => new Animation(TextureManager.GetTexture("cow_eat", 128, 64, 1), 64, 32, -0.5f, 0, 0.18f);

        public static Animation CowEatRight => new Animation(TextureManager.GetTexture("cow_eat", 128, 64, 3), 64, 32, -0.5f, 0, 0.18f);

        public static Animation CowEatUp => new Animation(TextureManager.GetTexture("cow_eat", 128, 64, 0), 32, 64, 0, -0.5f, 0.18f);

        public static Animation CowEatDown => new Animation(TextureManager.GetTexture("cow_eat", 128, 64, 2), 32, 64, 0, -0.5f, 0.18f);

        public static Animation PigEatLeft => new Animation(TextureManager.GetTexture("pig_eat", 128, 64, 1), 64, 32, -0.5f, 0, 0.18f);

        public static Animation PigEatRight => new Animation(TextureManager.GetTexture("pig_eat", 128, 64, 3), 64, 32, -0.5f, 0, 0.18f);

        public static Animation PigEatUp => new Animation(TextureManager.GetTexture("pig_eat", 128, 64, 0), 32, 64, 0, -0.5f, 0.18f);

        public static Animation PigEatDown => new Animation(TextureManager.GetTexture("pig_eat", 128, 64, 2), 32, 64, 0, -0.5f, 0.18f);

        public static Animation AttackChickenWalkUp => new Animation(TextureManager.GetTexture("attack_chicken_walk", 128, 32, 0), 32, 32, 0, 0, 0.1f);

        public static Animation AttackChickenWalkLeft => new Animation(TextureManager.GetTexture("attack_chicken_walk", 128, 32, 1), 32, 32, 0, 0, 0.1f);

        public static Animation AttackChickenWalkDown => new Animation(TextureManager.GetTexture("attack_chicken_walk", 128, 32, 2), 32, 32, 0, 0, 0.1f);

        public static Animation AttackChickenWalkRight => new Animation(TextureManager.GetTexture("attack_chicken_walk", 128, 32, 3), 32, 32, 0, 0, 0.1f);

        public static Animation AttackPigWalkUp => new Animation(TextureManager.GetTexture("attack_pig_walk", 128, 64, 0), 32, 64, 0, -0.5f, 0.1f);

        public static Animation AttackPigWalkLeft => new Animation(TextureManager.GetTexture("attack_pig_walk", 128, 64, 1), 64, 32, -0.5f, 0, 0.1f);

        public static Animation AttackPigWalkDown => new Animation(TextureManager.GetTexture("attack_pig_walk", 128, 64, 2), 32, 64, 0, -0.5f, 0.1f);

        public static Animation AttackPigWalkRight => new Animation(TextureManager.GetTexture("attack_pig_walk", 128, 64, 3), 64, 32, -0.5f, 0, 0.1f);

        public static Animation AttackCowWalkUp => new Animation(TextureManager.GetTexture("attack_cow_walk", 128, 64, 0), 32, 64, 0, -0.5f, 0.1f);

        public static Animation AttackCowWalkLeft => new Animation(TextureManager.GetTexture("attack_cow_walk", 128, 64, 1), 64, 32, -0.5f, 0, 0.1f);

        public static Animation AttackCowWalkDown => new Animation(TextureManager.GetTexture("attack_cow_walk", 128, 64, 2), 32, 64, 0, -0.5f, 0.1f);

        public static Animation AttackCowWalkRight => new Animation(TextureManager.GetTexture("attack_cow_walk", 128, 64, 3), 64, 32, -0.5f, 0, 0.1f);

        public static Animation AttackChickenEatLeft => new Animation(TextureManager.GetTexture("attack_chicken_eat", 128, 32, 1), 32, 32, 0, 0, 0.1f);

        public static Animation AttackChickenEatRight => new Animation(TextureManager.GetTexture("attack_chicken_eat", 128, 32, 3), 32, 32, 0, 0, 0.1f);

        public static Animation AttackChickenEatUp => new Animation(TextureManager.GetTexture("attack_chicken_eat", 128, 32, 0), 32, 32, 0, 0, 0.1f);

        public static Animation AttackChickenEatDown => new Animation(TextureManager.GetTexture("attack_chicken_eat", 128, 32, 2), 32, 32, 0, 0, 0.1f);

        public static Animation AttackCowEatLeft => new Animation(TextureManager.GetTexture("attack_cow_eat", 128, 64, 1), 64, 32, -0.5f, 0, 0.1f);

        public static Animation AttackCowEatRight => new Animation(TextureManager.GetTexture("attack_cow_eat", 128, 64, 3), 64, 32, -0.5f, 0, 0.1f);

        public static Animation AttackCowEatUp => new Animation(TextureManager.GetTexture("attack_cow_eat", 128, 64, 0), 32, 64, 0, -0.5f, 0.1f);

        public static Animation AttackCowEatDown => new Animation(TextureManager.GetTexture("attack_cow_eat", 128, 64, 2), 32, 64, 0, -0.5f, 0.1f);

        public static Animation AttackPigEatLeft => new Animation(TextureManager.GetTexture("attack_pig_eat", 128, 64, 1), 64, 32, -0.5f, 0, 0.1f);

        public static Animation AttackPigEatRight => new Animation(TextureManager.GetTexture("attack_pig_eat", 128, 64, 3), 64, 32, -0.5f, 0, 0.1f);

        public static Animation AttackPigEatUp => new Animation(TextureManager.GetTexture("attack_pig_eat", 128, 64, 0), 32, 64, 0, -0.5f, 0.1f);

        public static Animation AttackPigEatDown => new Animation(TextureManager.GetTexture("attack_pig_eat", 128, 64, 2), 32, 64, 0, -0.5f, 0.1f);
        
        public static Animation CoinSpin => new Animation(TextureManager.GetTexture("coin_spin"), 32, 32, 0.15f);

        public static Animation TrapClose => new Animation(TextureManager.GetTexture("trap"), 64, 64, 0.01f);

        public static Animation Blood => new Animation(TextureManager.GetTexture("blood"), 32, 32, 0.03f);

        /// <summary>
        /// Updated the AnimationManager.
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            foreach (var animation in sAnimations)
            {
                animation.Update(gameTime);
            }
        }

        public static Animation GetAnimation(Animation animation, object user)
        {
            if (animation == null) return null;
            sAnimations.Add(animation);
            if (!sAnimationUsers.Keys.Contains(user)) sAnimationUsers[user] = new List<Animation>();
            sAnimationUsers[user].Add(animation);
            return animation;
        }

        public static void RemoveAnimations(object user)
        {
            if (!sAnimationUsers.Keys.Contains(user)) return;
            foreach (var animation in sAnimationUsers[user])
            {
                sAnimations.Remove(animation);
            }
            sAnimationUsers.Remove(user);
        }
    }
}
