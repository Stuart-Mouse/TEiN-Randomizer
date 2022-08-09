using System;
using System.IO;

namespace TEiNRandomizer
{
    public static class Physics
    {
        public static string PlayerPhysics()
        {
            double gravity = 40;
            double leniency = .1;
            double max_fallspeed = -60;
            double maxspeed = 8;
            double runspeed = 11.5;
            double ground_friction = .7;
            double stop_friction = .1;
            double air_friction = .8;
            double fastfall_gravity = 65;
            double fastfall_jerkdown = -5;
            double fastfall_maxspeed = -90;
            double float_time = 1;
            double jumpheight = 3.2;
            double release_jumpheight = .3;
            double longjump_slidethreshold = 5;
            double longjump_speed = 16;
            double longjump_height = 1.9;
            double longjump_airfriction = .7;
            double ledgejump_height = 3;
            double enemy_bounceheight_nojump = .7;
            double enemy_bounceheight_jump = 4.2;
            double mush_bounceheight_nojump = 2.7;
            double mush_bounceheight_jump = 8.2;
            double enemy_prebounce_time = 999999;
            double enemy_postbounce_time = .05;
            double swimspeed = 0;
            double water_exitheight = 0;

            string extras = "";

            // bounds
            int lower = 25;
            int upper = 251;
            int maxBonusJumps = 3;

            // scalar values
            //double gravityScale = (double)RNG.rand.Next(25, 401) / 100;
            //double speedScale = (double)RNG.rand.Next(30, 201) / 100;

            gravity = RNG.random.Next(10, 121);
            max_fallspeed = -RNG.random.Next(50, 1000) / 10;

            maxspeed = RNG.random.Next(50, 201) / 10;
            runspeed = RNG.random.Next(50, 201) / 10;


            //double speedFriction;
            //if (speedScale > 1)
            //{
            //    speedFriction = speedScale / 100;
            //}

            //ground_friction *= Math.Min(0.9999, (double)RNG.rand.Next(lower, upper) / 100);
            //stop_friction *= Math.Min(0.9999, (double)RNG.rand.Next(lower, upper) / 100);
            //air_friction *= Math.Min(0.9999, (double)RNG.rand.Next(lower, upper) / 100);

            ground_friction = (double)RNG.random.Next(7000, 9000) / 10000;
            stop_friction = (double)RNG.random.Next(7000, 8500) / 10000;
            air_friction = (double)RNG.random.Next(7000, 10000) / 10000;

            fastfall_gravity = RNG.random.Next(10, 121);
            fastfall_jerkdown = -RNG.random.Next(1, 121) / 10;
            fastfall_maxspeed = -RNG.random.Next(1, 121);

            if (RNG.random.Next(0, 10) == 0)
            {
                fastfall_gravity  *= -1;
                fastfall_jerkdown *= -1;
                fastfall_maxspeed *= -1;
            }

            //float_time *= (double)RNG.rand.Next(lower, upper) / 100;
            jumpheight = (double)RNG.random.Next(320, 751) / 100;
            release_jumpheight = (double)RNG.random.Next(10, 70) / 100;
            
            //longjump_slidethreshold *= (double)RNG.rand.Next(lower, upper) / 100;
            longjump_speed = RNG.random.Next(100, 300) / 10;
            longjump_height = (double)RNG.random.Next(50, 601) / 100;
            longjump_airfriction = (double)RNG.random.Next(5000, 10000) / 10000;
            ledgejump_height = (double)RNG.random.Next(20, 60) / 10;

            //longjump_slidethreshold = ;

            enemy_bounceheight_nojump = (double)RNG.random.Next(2, 31) / 10;
            enemy_bounceheight_jump = (double)RNG.random.Next(20, 81) / 10;
            mush_bounceheight_nojump = (double)RNG.random.Next(10, 51) / 10;
            mush_bounceheight_jump = (double)RNG.random.Next(40, 101) / 10;
            //enemy_prebounce_time *= (double)RNG.rand.Next(lower, upper) / 100;
            //enemy_postbounce_time *= (double)RNG.rand.Next(lower, upper) / 100;
            //swimspeed *= (double)RNG.rand.Next(lower, upper) / 100;
            //water_exitheight *= (double)RNG.rand.Next(lower, upper) / 100;

            if (jumpheight < 3.5)
                maxBonusJumps = 1;
            if (RNG.CoinFlip()) // bonus jumps
            {
                double numBonusJumps = RNG.random.Next(1, maxBonusJumps + 1);
                jumpheight /= numBonusJumps + 1;

                extras += "bonus_jumps " + numBonusJumps + "\n";
                extras += "bonus_jump_heights [";
                for (int i = 0; i < numBonusJumps + 1; i++)
                {
                    if (i > 0)
                        extras += ", ";
                    extras += jumpheight;

                }
                extras += "]\n";
                if (numBonusJumps == 0)
                    leniency = RNG.random.NextDouble();
            }

            string filename = "data/player_physics/" + RNG.GetUInt32().ToString() + ".txt";

            using (StreamWriter sw = File.CreateText(Randomizer.SaveDir + filename))
            {
                sw.WriteLine(nameof(gravity) + " " + gravity);
                sw.WriteLine(nameof(leniency) + " " + leniency);
                sw.WriteLine(nameof(max_fallspeed) + " " + max_fallspeed);
                sw.WriteLine(nameof(maxspeed) + " " + maxspeed);
                sw.WriteLine(nameof(runspeed) + " " + runspeed);
                sw.WriteLine(nameof(ground_friction) + " " + ground_friction);
                sw.WriteLine(nameof(stop_friction) + " " + stop_friction);
                sw.WriteLine(nameof(air_friction) + " " + air_friction);
                sw.WriteLine(nameof(fastfall_gravity) + " " + fastfall_gravity);
                sw.WriteLine(nameof(fastfall_jerkdown) + " " + fastfall_jerkdown);
                sw.WriteLine(nameof(fastfall_maxspeed) + " " + fastfall_maxspeed);
                sw.WriteLine(nameof(float_time) + " " + float_time);
                sw.WriteLine(nameof(jumpheight) + " " + jumpheight);
                sw.WriteLine(nameof(release_jumpheight) + " " + release_jumpheight);
                sw.WriteLine(nameof(longjump_slidethreshold) + " " + longjump_slidethreshold);
                sw.WriteLine(nameof(longjump_speed) + " " + longjump_speed);
                sw.WriteLine(nameof(longjump_height) + " " + longjump_height);
                sw.WriteLine(nameof(longjump_airfriction) + " " + longjump_airfriction);
                sw.WriteLine(nameof(ledgejump_height) + " " + ledgejump_height);
                sw.WriteLine(nameof(enemy_bounceheight_nojump) + " " + enemy_bounceheight_nojump);
                sw.WriteLine(nameof(enemy_bounceheight_jump) + " " + enemy_bounceheight_jump);
                sw.WriteLine(nameof(mush_bounceheight_nojump) + " " + mush_bounceheight_nojump);
                sw.WriteLine(nameof(mush_bounceheight_jump) + " " + mush_bounceheight_jump);
                sw.WriteLine(nameof(enemy_prebounce_time) + " " + enemy_prebounce_time);
                sw.WriteLine(nameof(enemy_postbounce_time) + " " + enemy_postbounce_time);
                sw.WriteLine(nameof(swimspeed) + " " + swimspeed);
                sw.WriteLine(nameof(water_exitheight) + " " + water_exitheight);
                sw.WriteLine(extras);
            }
            return (filename);
        }
        public static string LowGravPhysics()
        {
            double gravity = 2;
            double leniency = .1;
            double max_fallspeed = -5;
            double maxspeed = 8;
            double runspeed = 11.5;
            double ground_friction = .7;
            double stop_friction = .1;
            double air_friction = .8;
            double fastfall_gravity = 8;
            double fastfall_jerkdown = -5;
            double fastfall_maxspeed = -12;
            double float_time = 1;
            double jumpheight = 3.2;
            double release_jumpheight = .3;
            double longjump_slidethreshold = 5;
            double longjump_speed = 16;
            double longjump_height = 1.9;
            double longjump_airfriction = .7;
            double ledgejump_height = 3;
            double enemy_bounceheight_nojump = .7;
            double enemy_bounceheight_jump = 4.2;
            double mush_bounceheight_nojump = 2.7;
            double mush_bounceheight_jump = 8.2;
            double enemy_prebounce_time = 999999;
            double enemy_postbounce_time = .05;
            double swimspeed = 8;
            double water_exitheight = 0;

            int lower = 50;
            int upper = 201;

            gravity *= (double)RNG.random.Next(lower, upper) / 100;
            leniency *= (double)RNG.random.Next(lower, upper) / 100;
            max_fallspeed *= (double)RNG.random.Next(lower, upper) / 100;
            maxspeed *= (double)RNG.random.Next(lower, upper) / 100;
            runspeed *= (double)RNG.random.Next(lower, upper) / 100;

            ground_friction = (double)RNG.random.Next(7000, 9000) / 10000;
            stop_friction = (double)RNG.random.Next(7000, 8500) / 10000;
            air_friction = (double)RNG.random.Next(7000, 10000) / 10000;

            fastfall_gravity *= (double)RNG.random.Next(lower, upper) / 100;
            fastfall_jerkdown *= (double)RNG.random.Next(lower, upper) / 100;
            fastfall_maxspeed *= (double)RNG.random.Next(lower, upper) / 100;
            float_time *= (double)RNG.random.Next(lower, upper) / 100;
            jumpheight *= (double)RNG.random.Next(lower, upper) / 100;
            release_jumpheight *= (double)RNG.random.Next(lower, upper) / 100;
            longjump_slidethreshold *= (double)RNG.random.Next(lower, upper) / 100;
            longjump_speed *= (double)RNG.random.Next(lower, upper) / 100;
            longjump_height *= (double)RNG.random.Next(lower, upper) / 100;
            longjump_airfriction *= (double)RNG.random.Next(lower, upper) / 100;
            ledgejump_height *= (double)RNG.random.Next(lower, upper) / 100;
            //enemy_bounceheight_nojump *= (double)RNG.rand.Next(lower, upper) / 100;
            //enemy_bounceheight_jump *= (double)RNG.rand.Next(lower, upper) / 100;
            //mush_bounceheight_nojump *= (double)RNG.rand.Next(lower, upper) / 100;
            //mush_bounceheight_jump *= (double)RNG.rand.Next(lower, upper) / 100;
            //enemy_prebounce_time *= (double)RNG.rand.Next(lower, upper) / 100;
            //enemy_postbounce_time *= (double)RNG.rand.Next(lower, upper) / 100;
            swimspeed *= (double)RNG.random.Next(lower, upper) / 100;
            water_exitheight *= (double)RNG.random.Next(lower, upper) / 100;

            string filename = "data/lowgrav_physics/" + RNG.GetUInt32().ToString() + ".txt";

            using (StreamWriter sw = File.CreateText(Randomizer.SaveDir + filename))
            {
                sw.WriteLine(nameof(gravity) + " " + gravity);
                sw.WriteLine(nameof(leniency) + " " + leniency);
                sw.WriteLine(nameof(max_fallspeed) + " " + max_fallspeed);
                sw.WriteLine(nameof(maxspeed) + " " + maxspeed);
                sw.WriteLine(nameof(runspeed) + " " + runspeed);
                sw.WriteLine(nameof(ground_friction) + " " + ground_friction);
                sw.WriteLine(nameof(stop_friction) + " " + stop_friction);
                sw.WriteLine(nameof(air_friction) + " " + air_friction);
                sw.WriteLine(nameof(fastfall_gravity) + " " + fastfall_gravity);
                sw.WriteLine(nameof(fastfall_jerkdown) + " " + fastfall_jerkdown);
                sw.WriteLine(nameof(fastfall_maxspeed) + " " + fastfall_maxspeed);
                sw.WriteLine(nameof(float_time) + " " + float_time);
                sw.WriteLine(nameof(jumpheight) + " " + jumpheight);
                sw.WriteLine(nameof(release_jumpheight) + " " + release_jumpheight);
                sw.WriteLine(nameof(longjump_slidethreshold) + " " + longjump_slidethreshold);
                sw.WriteLine(nameof(longjump_speed) + " " + longjump_speed);
                sw.WriteLine(nameof(longjump_height) + " " + longjump_height);
                sw.WriteLine(nameof(longjump_airfriction) + " " + longjump_airfriction);
                sw.WriteLine(nameof(ledgejump_height) + " " + ledgejump_height);
                sw.WriteLine(nameof(enemy_bounceheight_nojump) + " " + enemy_bounceheight_nojump);
                sw.WriteLine(nameof(enemy_bounceheight_jump) + " " + enemy_bounceheight_jump);
                sw.WriteLine(nameof(mush_bounceheight_nojump) + " " + mush_bounceheight_nojump);
                sw.WriteLine(nameof(mush_bounceheight_jump) + " " + mush_bounceheight_jump);
                sw.WriteLine(nameof(enemy_prebounce_time) + " " + enemy_prebounce_time);
                sw.WriteLine(nameof(enemy_postbounce_time) + " " + enemy_postbounce_time);
                sw.WriteLine(nameof(swimspeed) + " " + swimspeed);
                sw.WriteLine(nameof(water_exitheight) + " " + water_exitheight);
            }
            return (filename);
        }
        public static string WaterPhysics()
        {
            double gravity = 28;
            double leniency = .1;
            double max_fallspeed = -5;
            double maxspeed = 5;
            double runspeed = 7;
            double ground_friction = .95;
            double stop_friction = .90;
            double air_friction = .95;
            double fastfall_gravity = 30;
            double fastfall_jerkdown = -1;
            double fastfall_maxspeed = -15;
            double float_time = 1;
            double jumpheight = 3.2;
            double release_jumpheight = .3;
            double longjump_slidethreshold = 5;
            double longjump_speed = 10;
            double longjump_height = 1.9;
            double longjump_airfriction = .99;
            double ledgejump_height = 3;
            double enemy_bounceheight_nojump = .7;
            double enemy_bounceheight_jump = 4.2;
            double mush_bounceheight_nojump = 2.7;
            double mush_bounceheight_jump = 8.2;
            double enemy_prebounce_time = 999999;
            double enemy_postbounce_time = .05;
            double swimspeed = 7.5;
            double water_exitheight = 2.6;

            int lower = 50;
            int upper = 201;

            gravity                 *= (double)RNG.random.Next(lower, upper) / 100;
            leniency                *= (double)RNG.random.Next(lower, upper) / 100;
            max_fallspeed           *= (double)RNG.random.Next(75, upper) / 100;
            maxspeed                *= (double)RNG.random.Next(lower, upper) / 100;
            runspeed                *= (double)RNG.random.Next(lower, upper) / 100;

            ground_friction = (double)RNG.random.Next(7000, 9000) / 10000;
            stop_friction = (double)RNG.random.Next(7000, 8500) / 10000;
            air_friction = (double)RNG.random.Next(7000, 10000) / 10000;

            fastfall_gravity        *= (double)RNG.random.Next(lower, upper) / 100;
            fastfall_jerkdown       *= (double)RNG.random.Next(lower, upper) / 100;
            fastfall_maxspeed       *= (double)RNG.random.Next(lower, upper) / 100;
            float_time              *= (double)RNG.random.Next(lower, upper) / 100;
            jumpheight              *= (double)RNG.random.Next(lower, upper) / 100;
            release_jumpheight      *= (double)RNG.random.Next(lower, upper) / 100;
            longjump_slidethreshold *= (double)RNG.random.Next(lower, upper) / 100;
            longjump_speed          *= (double)RNG.random.Next(lower, upper) / 100;
            longjump_height         *= (double)RNG.random.Next(lower, upper) / 100;
            longjump_airfriction    *= (double)RNG.random.Next(lower, upper) / 100;
            ledgejump_height        *= (double)RNG.random.Next(lower, upper) / 100;
            //enemy_bounceheight_nojump *= (double)RNG.rand.Next(lower, upper) / 100;
            //enemy_bounceheight_jump *= (double)RNG.rand.Next(lower, upper) / 100;
            //mush_bounceheight_nojump *= (double)RNG.rand.Next(lower, upper) / 100;
            //mush_bounceheight_jump *= (double)RNG.rand.Next(lower, upper) / 100;
            //enemy_prebounce_time *= (double)RNG.rand.Next(lower, upper) / 100;
            //enemy_postbounce_time *= (double)RNG.rand.Next(lower, upper) / 100;
            swimspeed *= (double)RNG.random.Next(lower, upper) / 100;
            water_exitheight *= (double)RNG.random.Next(lower, upper) / 100;

            string filename = "data/water_physics/" + RNG.GetUInt32().ToString() + ".txt";

            using (StreamWriter sw = File.CreateText(Randomizer.SaveDir + filename))
            {
                sw.WriteLine(nameof(gravity) + " " + gravity);
                sw.WriteLine(nameof(leniency) + " " + leniency);
                sw.WriteLine(nameof(max_fallspeed) + " " + max_fallspeed);
                sw.WriteLine(nameof(maxspeed) + " " + maxspeed);
                sw.WriteLine(nameof(runspeed) + " " + runspeed);
                sw.WriteLine(nameof(ground_friction) + " " + ground_friction);
                sw.WriteLine(nameof(stop_friction) + " " + stop_friction);
                sw.WriteLine(nameof(air_friction) + " " + air_friction);
                sw.WriteLine(nameof(fastfall_gravity) + " " + fastfall_gravity);
                sw.WriteLine(nameof(fastfall_jerkdown) + " " + fastfall_jerkdown);
                sw.WriteLine(nameof(fastfall_maxspeed) + " " + fastfall_maxspeed);
                sw.WriteLine(nameof(float_time) + " " + float_time);
                sw.WriteLine(nameof(jumpheight) + " " + jumpheight);
                sw.WriteLine(nameof(release_jumpheight) + " " + release_jumpheight);
                sw.WriteLine(nameof(longjump_slidethreshold) + " " + longjump_slidethreshold);
                sw.WriteLine(nameof(longjump_speed) + " " + longjump_speed);
                sw.WriteLine(nameof(longjump_height) + " " + longjump_height);
                sw.WriteLine(nameof(longjump_airfriction) + " " + longjump_airfriction);
                sw.WriteLine(nameof(ledgejump_height) + " " + ledgejump_height);
                sw.WriteLine(nameof(enemy_bounceheight_nojump) + " " + enemy_bounceheight_nojump);
                sw.WriteLine(nameof(enemy_bounceheight_jump) + " " + enemy_bounceheight_jump);
                sw.WriteLine(nameof(mush_bounceheight_nojump) + " " + mush_bounceheight_nojump);
                sw.WriteLine(nameof(mush_bounceheight_jump) + " " + mush_bounceheight_jump);
                sw.WriteLine(nameof(enemy_prebounce_time) + " " + enemy_prebounce_time);
                sw.WriteLine(nameof(enemy_postbounce_time) + " " + enemy_postbounce_time);
                sw.WriteLine(nameof(swimspeed) + " " + swimspeed);
                sw.WriteLine(nameof(water_exitheight) + " " + water_exitheight);
            }
                return (filename);
        }
        public static string PlatformPhysics()
        {
            // DEFAULT PLATFORM PHYSICS
            double MovingPlatformSpeedLow = 2;
            double MovingPlatformSpeedMed = 4;
            double MovingPlatformSpeedHigh = 6;
            double SmoothMovingPlatformSpeedLow = 1;
            double SmoothMovingPlatformSpeedMed = 3;
            double SmoothMovingPlatformSpeedHigh = 5;
            double SmoothMovingPlatformSpeedXSlow = .2;
            double FallingPlatformGravitytLow = .5;
            double FallingPlatformGravitytMed = 1;
            double FallingPlatformGravitytHigh = 2;
            double CrumblingPlatformGravitytLow = .5;
            double CrumblingPlatformGravitytMed = 1;
            double CrumblingPlatformGravitytHigh = 2;
            double SmasherSpeedLow = 20;
            double SmasherDelayLow = 2;
            double SmasherSpeedMed = 25;
            double SmasherDelayMed = 1;
            double SmasherSpeedHigh = 30;
            double SmasherDelayHigh = .5;
            double WeightedPlatformSpeedLow = 1;
            double WeightedPlatformSpeedMed = 2;
            double WeightedPlatformSpeedHigh = 4;

            // Lower and Upper Bounds (in %)
            double minimum = 0;
            int lower = 30;
            int upper = 201;

            // Alter Values
            MovingPlatformSpeedLow *= (double)RNG.random.Next(lower, upper) / 100;
            MovingPlatformSpeedMed *= (double)RNG.random.Next(lower, upper) / 100;
            MovingPlatformSpeedHigh *= (double)RNG.random.Next(lower, upper) / 100;
            SmoothMovingPlatformSpeedLow *= (double)RNG.random.Next(lower, upper) / 100;
            SmoothMovingPlatformSpeedMed *= (double)RNG.random.Next(lower, upper) / 100;
            SmoothMovingPlatformSpeedHigh *= (double)RNG.random.Next(lower, upper) / 100;
            SmoothMovingPlatformSpeedXSlow *= (double)RNG.random.Next(lower, upper) / 100;
            FallingPlatformGravitytLow *= (double)RNG.random.Next(lower, upper) / 100;
            FallingPlatformGravitytMed *= (double)RNG.random.Next(lower, upper) / 100;
            FallingPlatformGravitytHigh *= (double)RNG.random.Next(lower, upper) / 100;
            CrumblingPlatformGravitytLow *= (double)RNG.random.Next(lower, upper) / 100;
            CrumblingPlatformGravitytMed *= (double)RNG.random.Next(lower, upper) / 100;
            CrumblingPlatformGravitytHigh *= (double)RNG.random.Next(lower, upper) / 100;
            SmasherSpeedLow = (double)RNG.random.Next(5, 40);
            SmasherDelayLow *= (double)RNG.random.Next(lower, upper) / 100;
            SmasherSpeedMed = (double)RNG.random.Next(7, 48);
            SmasherDelayMed *= (double)RNG.random.Next(lower, upper) / 100;
            SmasherSpeedHigh = (double)RNG.random.Next(10, 56);
            SmasherDelayHigh *= (double)RNG.random.Next(lower, upper) / 100;
            WeightedPlatformSpeedLow *= (double)RNG.random.Next(lower, upper) / 100;
            WeightedPlatformSpeedMed *= (double)RNG.random.Next(lower, upper) / 100;
            WeightedPlatformSpeedHigh *= (double)RNG.random.Next(lower, upper) / 100;

            // Write platformphysics.txt
            string filename = "data/platform_physics/" + RNG.GetUInt32().ToString() + ".txt";
            using (StreamWriter sw = File.CreateText(Randomizer.SaveDir + filename))
            {
                sw.WriteLine(nameof(MovingPlatformSpeedLow) + " " + MovingPlatformSpeedLow);
                sw.WriteLine(nameof(MovingPlatformSpeedMed) + " " + MovingPlatformSpeedMed);
                sw.WriteLine(nameof(MovingPlatformSpeedHigh) + " " + MovingPlatformSpeedHigh);
                sw.WriteLine(nameof(SmoothMovingPlatformSpeedLow) + " " + SmoothMovingPlatformSpeedLow);
                sw.WriteLine(nameof(SmoothMovingPlatformSpeedMed) + " " + SmoothMovingPlatformSpeedMed);
                sw.WriteLine(nameof(SmoothMovingPlatformSpeedHigh) + " " + SmoothMovingPlatformSpeedHigh);
                sw.WriteLine(nameof(SmoothMovingPlatformSpeedXSlow) + " " + SmoothMovingPlatformSpeedXSlow);
                sw.WriteLine(nameof(FallingPlatformGravitytLow) + " " + FallingPlatformGravitytLow);
                sw.WriteLine(nameof(FallingPlatformGravitytMed) + " " + FallingPlatformGravitytMed);
                sw.WriteLine(nameof(FallingPlatformGravitytHigh) + " " + FallingPlatformGravitytHigh);
                sw.WriteLine(nameof(CrumblingPlatformGravitytLow) + " " + CrumblingPlatformGravitytLow);
                sw.WriteLine(nameof(CrumblingPlatformGravitytMed) + " " + CrumblingPlatformGravitytMed);
                sw.WriteLine(nameof(CrumblingPlatformGravitytHigh) + " " + CrumblingPlatformGravitytHigh);
                sw.WriteLine(nameof(SmasherSpeedLow) + " " + SmasherSpeedLow);
                sw.WriteLine(nameof(SmasherDelayLow) + " " + SmasherDelayLow);
                sw.WriteLine(nameof(SmasherSpeedMed) + " " + SmasherSpeedMed);
                sw.WriteLine(nameof(SmasherDelayMed) + " " + SmasherDelayMed);
                sw.WriteLine(nameof(SmasherSpeedHigh) + " " + SmasherSpeedHigh);
                sw.WriteLine(nameof(SmasherDelayHigh) + " " + SmasherDelayHigh);
                sw.WriteLine(nameof(WeightedPlatformSpeedLow) + " " + WeightedPlatformSpeedLow);
                sw.WriteLine(nameof(WeightedPlatformSpeedMed) + " " + WeightedPlatformSpeedMed);
                sw.WriteLine(nameof(WeightedPlatformSpeedHigh) + " " + WeightedPlatformSpeedHigh);
            }

            return (filename);
        }



    }
}
