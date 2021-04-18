using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;

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
            //double gravityScale = (double)Randomizer.myRNG.rand.Next(25, 401) / 100;
            //double speedScale = (double)Randomizer.myRNG.rand.Next(30, 201) / 100;

            gravity = Randomizer.myRNG.rand.Next(10, 121);
            max_fallspeed = -Randomizer.myRNG.rand.Next(50, 1000) / 10;

            maxspeed = Randomizer.myRNG.rand.Next(50, 201) / 10;
            runspeed = Randomizer.myRNG.rand.Next(50, 201) / 10;


            //double speedFriction;
            //if (speedScale > 1)
            //{
            //    speedFriction = speedScale / 100;
            //}

            //ground_friction *= Math.Min(0.9999, (double)Randomizer.myRNG.rand.Next(lower, upper) / 100);
            //stop_friction *= Math.Min(0.9999, (double)Randomizer.myRNG.rand.Next(lower, upper) / 100);
            //air_friction *= Math.Min(0.9999, (double)Randomizer.myRNG.rand.Next(lower, upper) / 100);

            ground_friction = (double)Randomizer.myRNG.rand.Next(7000, 9000) / 10000;
            stop_friction = (double)Randomizer.myRNG.rand.Next(7000, 8500) / 10000;
            air_friction = (double)Randomizer.myRNG.rand.Next(7000, 10000) / 10000;

            fastfall_gravity = Randomizer.myRNG.rand.Next(10, 121);
            fastfall_jerkdown = -Randomizer.myRNG.rand.Next(1, 121) / 10;
            fastfall_maxspeed = -Randomizer.myRNG.rand.Next(1, 121);

            if (Randomizer.myRNG.rand.Next(0, 10) == 0)
            {
                fastfall_gravity *= -1;
                fastfall_jerkdown *= -1;
                fastfall_maxspeed *= -1;
            }

            //float_time *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            jumpheight = (double)Randomizer.myRNG.rand.Next(320, 751) / 100;
            release_jumpheight = (double)Randomizer.myRNG.rand.Next(10, 70) / 100;
            
            //longjump_slidethreshold *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            longjump_speed = Randomizer.myRNG.rand.Next(100, 300) / 10;
            longjump_height = (double)Randomizer.myRNG.rand.Next(50, 601) / 100;
            longjump_airfriction = (double)Randomizer.myRNG.rand.Next(5000, 10000) / 10000;
            ledgejump_height = (double)Randomizer.myRNG.rand.Next(20, 60) / 10;

            //longjump_slidethreshold = ;

            enemy_bounceheight_nojump = (double)Randomizer.myRNG.rand.Next(2, 31) / 10;
            enemy_bounceheight_jump = (double)Randomizer.myRNG.rand.Next(20, 81) / 10;
            mush_bounceheight_nojump = (double)Randomizer.myRNG.rand.Next(10, 51) / 10;
            mush_bounceheight_jump = (double)Randomizer.myRNG.rand.Next(40, 101) / 10;
            //enemy_prebounce_time *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //enemy_postbounce_time *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //swimspeed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //water_exitheight *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;

            if (jumpheight < 3.5)
                maxBonusJumps = 1;
            if (Randomizer.myRNG.CoinFlip()) // bonus jumps
            {
                double numBonusJumps = Randomizer.myRNG.rand.Next(1, maxBonusJumps + 1);
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
                    leniency = Randomizer.myRNG.rand.NextDouble();
            }

            string filename = "data/player_physics/" + Randomizer.myRNG.GetUInt32().ToString() + ".txt";

            using (StreamWriter sw = File.CreateText(Randomizer.settings.GameDirectory + filename))
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

            gravity *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            leniency *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            max_fallspeed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            maxspeed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            runspeed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            ground_friction *= Math.Min(0.99, (double)Randomizer.myRNG.rand.Next(lower, upper) / 100);
            stop_friction *= Math.Min(0.99, (double)Randomizer.myRNG.rand.Next(lower, upper) / 100);
            air_friction *= Math.Min(0.99, (double)Randomizer.myRNG.rand.Next(lower, upper) / 100);
            fastfall_gravity *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            fastfall_jerkdown *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            fastfall_maxspeed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            float_time *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            jumpheight *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            release_jumpheight *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            longjump_slidethreshold *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            longjump_speed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            longjump_height *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            longjump_airfriction *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            ledgejump_height *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //enemy_bounceheight_nojump *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //enemy_bounceheight_jump *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //mush_bounceheight_nojump *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //mush_bounceheight_jump *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //enemy_prebounce_time *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //enemy_postbounce_time *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            swimspeed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            water_exitheight *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;

            string filename = "data/lowgrav_physics/" + Randomizer.myRNG.GetUInt32().ToString() + ".txt";

            using (StreamWriter sw = File.CreateText(Randomizer.settings.GameDirectory + filename))
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

            gravity *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            leniency *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            max_fallspeed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            maxspeed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            runspeed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            ground_friction *= Math.Min(0.99, (double)Randomizer.myRNG.rand.Next(lower, upper) / 100);
            stop_friction *= Math.Min(0.99, (double)Randomizer.myRNG.rand.Next(lower, upper) / 100);
            air_friction *= Math.Min(0.99, (double)Randomizer.myRNG.rand.Next(lower, upper) / 100);
            fastfall_gravity *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            fastfall_jerkdown *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            fastfall_maxspeed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            float_time *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            jumpheight *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            release_jumpheight *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            longjump_slidethreshold *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            longjump_speed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            longjump_height *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            longjump_airfriction *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            ledgejump_height *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //enemy_bounceheight_nojump *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //enemy_bounceheight_jump *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //mush_bounceheight_nojump *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //mush_bounceheight_jump *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //enemy_prebounce_time *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            //enemy_postbounce_time *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            swimspeed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            water_exitheight *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;

            string filename = "data/water_physics/" + Randomizer.myRNG.GetUInt32().ToString() + ".txt";

            using (StreamWriter sw = File.CreateText(Randomizer.settings.GameDirectory + filename))
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
            int upper = 301;

            // Alter Values
            MovingPlatformSpeedLow *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            MovingPlatformSpeedMed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            MovingPlatformSpeedHigh *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            SmoothMovingPlatformSpeedLow *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            SmoothMovingPlatformSpeedMed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            SmoothMovingPlatformSpeedHigh *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            SmoothMovingPlatformSpeedXSlow *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            FallingPlatformGravitytLow *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            FallingPlatformGravitytMed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            FallingPlatformGravitytHigh *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            CrumblingPlatformGravitytLow *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            CrumblingPlatformGravitytMed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            CrumblingPlatformGravitytHigh *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            SmasherSpeedLow *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            SmasherDelayLow *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            SmasherSpeedMed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            SmasherDelayMed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            SmasherSpeedHigh *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            SmasherDelayHigh *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            WeightedPlatformSpeedLow *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            WeightedPlatformSpeedMed *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;
            WeightedPlatformSpeedHigh *= (double)Randomizer.myRNG.rand.Next(lower, upper) / 100;

            // Write platformphysics.txt
            string filename = "data/platform_physics/" + Randomizer.myRNG.GetUInt32().ToString() + ".txt";
            using (StreamWriter sw = File.CreateText(Randomizer.settings.GameDirectory + filename))
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
