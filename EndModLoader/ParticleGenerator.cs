using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace TEiNRandomizer
{
    public static class ParticleGenerator
    {
        public static string GetParticle(RandomizerSettings settings)
        {
            string particle_name = "";
            switch (RNG.random.Next(0, 2))
            {
                case 0:
                    particle_name = DirectionParticle(settings);
                    break;
                case 1:
                    particle_name = MistParticle(settings);
                    break;
            }
            return particle_name;
        }
        public static string MistParticle(RandomizerSettings settings)
        {
            string particle_name = "MistParticle" + RNG.GetUInt32().ToString();

            int layer = 4;
            int base_speed = 3;

            // can be anything
            double alpha_start = 1;
            double alpha_end = 0;
            string movieclip;
            int max_particles = settings.MaxParticles;
            int emit_spread = 0;
            string rotation_speed = "0";
            string initial_rotation = "0";
            double particle_lifetime = RNG.random.Next(1, 5);
            string size_start;
            int size_end = RNG.random.Next(50, 101);

            string emit_direction = $"[{RNG.random.Next(-10, 11) / 10},{RNG.random.Next(-10, 11) / 10}]";
            if (emit_direction == "[0,0]")
                emit_direction = "[0,-1]";
            //string emit_box = "";
            //string emit_offset = "";

            string force = "0";
            int emit_rate;
            int emit_amount;
            double initial_speed = RNG.random.Next(0, 20);
            double friction = 1;

            // select particle from pool
            var doc = XDocument.Load($"data/particles_templates.xml");    // open particle file
            string template = doc.Root.Element("templates").Element("MistParticle").Value;
            var particles = doc.Root.Element("particles").Elements();
            var chosen = particles.ElementAt(RNG.random.Next(0, particles.Count()));
            movieclip = chosen.Attribute("name").Value;
            string face_moving_direction = chosen.Attribute("face_moving_direction").Value;
            alpha_start = Convert.ToDouble(chosen.Attribute("alpha").Value);
            double size = Convert.ToDouble(chosen.Attribute("size").Value) * .03;
            size_start = $"[{size - size * .25},{size + size * .25}]";
            layer = Convert.ToInt32(chosen.Attribute("layer").Value);
            double density = Convert.ToDouble(chosen.Attribute("density").Value);
            double speed_scale = Convert.ToDouble(chosen.Attribute("speed_scale").Value);

            int emit_density = (int)(64 * density);
            emit_rate = RNG.random.Next(1, emit_density);
            emit_amount = emit_density / emit_rate;
            if (RNG.CoinFlip())
                emit_spread = RNG.random.Next(0, 46);
            if (RNG.random.Next(0, 10) == 0)
                emit_spread = 360;

            if (!Convert.ToBoolean(face_moving_direction))
            {
                int temp = RNG.random.Next(0, 251);
                rotation_speed = $"[{temp},{temp * 1.5}]";
                initial_rotation = "[0, 359]";
            }

            template = template.Replace("PARTICLE_NAME", particle_name);
            template = template.Replace("LAYER", layer.ToString());
            template = template.Replace("MOVIECLIP", movieclip);
            template = template.Replace("MAX_PARTICLES", max_particles.ToString());
            template = template.Replace("EMIT_RATE", emit_rate.ToString());
            template = template.Replace("EMIT_AMOUNT", emit_amount.ToString());
            template = template.Replace("EMIT_DIRECTION", emit_direction);
            template = template.Replace("EMIT_SPREAD", emit_spread.ToString());
            //template = template.Replace("EMIT_BOX", emit_box);
            //template = template.Replace("EMIT_OFFSET", emit_offset);
            template = template.Replace("PARTICLE_LIFETIME", particle_lifetime.ToString());
            template = template.Replace("INITIAL_SPEED", $"[{initial_speed},{initial_speed * 2}]");
            template = template.Replace("INITIAL_ROTATION", initial_rotation.ToString());
            template = template.Replace("ROTATION_SPEED", rotation_speed.ToString());
            template = template.Replace("FORCE", force);
            template = template.Replace("FRICTION", friction.ToString());
            template = template.Replace("ALPHA_START", alpha_start.ToString());
            template = template.Replace("ALPHA_END", alpha_end.ToString());
            template = template.Replace("SIZE_START", size_start);
            template = template.Replace("SIZE_END", size_end.ToString());
            template = template.Replace("FACE_MOVING_DIRECTION", face_moving_direction);
            template = template.Replace("SPEED_SCALE", speed_scale.ToString());

            using (StreamWriter sw = File.AppendText(Randomizer.saveDir + "data/particles.txt.append"))
            {
                sw.WriteLine(template);
            }
            return particle_name;
        }

        public static string DirectionParticle(RandomizerSettings settings)
        {
            string particle_name = "DirectionParticle" + RNG.GetUInt32().ToString();

            int layer = 4;
            double base_speed = 3;
            double base_force = 1;
            string notes = "#";

            // can be anything
            double alpha_start = 1;
            double alpha_end = 1;
            string movieclip;
            int max_particles = settings.MaxParticles;
            int emit_spread = 0;
            string rotation_speed = "0";
            string initial_rotation = "0";
            double particle_lifetime = 80;
            string size_start;

            // establish direction
            string emit_direction = "";
            string emit_box = "";
            string emit_offset = "";

            // dependent on eachother
            int emit_rate;
            int emit_amount;
            double initial_speed;
            double friction = 1;
            string force;

            // select particle from pool
            var doc = XDocument.Load($"data/particles_templates.xml");    // open particle file
            string template = doc.Root.Element("templates").Element("DirectionParticle").Value;
            var particles = doc.Root.Element("particles").Elements();
            var chosen = particles.ElementAt(RNG.random.Next(0, particles.Count()));
            movieclip = chosen.Attribute("name").Value;
            string face_moving_direction = chosen.Attribute("face_moving_direction").Value;
            alpha_start = Convert.ToDouble(chosen.Attribute("alpha").Value);
            double size = Convert.ToDouble(chosen.Attribute("size").Value);
            size_start = $"[{size - size * .25},{size + size * .25}]";
            layer = Convert.ToInt32(chosen.Attribute("layer").Value);
            double density = Convert.ToDouble(chosen.Attribute("density").Value);
            double speed_scale = Convert.ToDouble(chosen.Attribute("speed_scale").Value);

            speed_scale += RNG.random.Next(-2, 3) / 10;

            int speed_scalar = 1, force_scalar = 1;
            // set moving direction
            bool force_neg = false;
            int direction = RNG.random.Next(0, 4);
            switch (direction)
            {
                case 0: // up
                    emit_direction = $"[{RNG.random.Next(-3, 4) / 10},1]";
                    emit_box = "[54,1]";
                    emit_offset = "[27,-3]";
                    break;
                case 1: // down
                    emit_direction = $"[{RNG.random.Next(-3, 4) / 10},-1]";
                    emit_box = "[54,1]";
                    emit_offset = "[27,35]";
                    force_neg = true;
                    break;
                case 2: // right
                    emit_direction = $"[1,{RNG.random.Next(-3, 4) / 10}]";
                    emit_box = "[1,32]";
                    emit_offset = "[-3,16]";
                    base_speed *= 1.2;
                    base_force *= .8;
                    break;
                case 3: // left
                    emit_direction = $"[-1,{RNG.random.Next(-3, 4) / 10}]";
                    emit_box = "[1,32]";
                    emit_offset = "[57,16]";
                    base_speed *= 1.2;
                    base_force *= .8;
                    force_neg = true;
                    break;
            }

            //int particle_density = 100;

            int emit_density = (int)(RNG.random.Next(5, 16) * density);
            speed_scalar = RNG.random.Next(1, 5);

            if (RNG.CoinFlip())   // decide whether to accelerate, slow down, or no force
            {
                force_scalar = speed_scalar * speed_scalar;
                if (RNG.CoinFlip() && direction < 2)   // gravity
                {
                    //force_scalar = speed_scalar * speed_scalar;
                    force_neg = !force_neg;
                    emit_density *= Math.Max(speed_scalar / 2, 1);
                    notes += "gravity, ";
                }
                else    // accelerate
                {
                    //force_scalar = speed_scalar * speed_scalar;
                    emit_density *= speed_scalar;
                    notes += "accelerate, ";
                }
            }
            else
            {
                force_scalar = 0;
                emit_density *= speed_scalar;
            }

            notes += $"force scalar: {force_scalar}, speed scalar: {speed_scalar}, ";

            initial_speed = base_speed * speed_scalar;  // set initial speed
            double parallel_force = base_force * force_scalar * -(Convert.ToInt32(force_neg));  // set force
            double perpendicular_force = base_force * force_scalar * RNG.random.Next(-10, 11) / 10;
            //perpendicular_force = 0;

            if (RNG.random.Next(0, 3) == 0)   // decide if particle fades out
            {
                alpha_end = 0;
                particle_lifetime = (float)RNG.random.Next(2, 4) * (float)base_speed / (float)speed_scalar;
            }

            if (direction > 1)  // if right or left
            {
                force = $"[{parallel_force},{perpendicular_force}]";
                particle_lifetime *= 2;
            }
            else force = $"[{perpendicular_force},{parallel_force}]";

            emit_rate = RNG.random.Next(1, emit_density);
            emit_amount = emit_density / emit_rate;
            if (RNG.CoinFlip())
                emit_spread = RNG.random.Next(0, 46);

            if (!Convert.ToBoolean(face_moving_direction))
            {
                int temp = RNG.random.Next(0, 251);
                rotation_speed = $"[{temp},{temp * 1.5}]";
                initial_rotation = "[0, 359]";
            }

            // write all values to template
            template = template.Replace("PARTICLE_NAME", particle_name);
            template = template.Replace("LAYER", layer.ToString());
            template = template.Replace("MOVIECLIP", movieclip);
            template = template.Replace("MAX_PARTICLES", max_particles.ToString());
            template = template.Replace("EMIT_RATE", emit_rate.ToString());
            template = template.Replace("EMIT_AMOUNT", emit_amount.ToString());
            template = template.Replace("EMIT_DIRECTION", emit_direction);
            template = template.Replace("EMIT_SPREAD", emit_spread.ToString());
            template = template.Replace("EMIT_BOX", emit_box);
            template = template.Replace("EMIT_OFFSET", emit_offset);
            template = template.Replace("PARTICLE_LIFETIME", particle_lifetime.ToString());
            template = template.Replace("INITIAL_SPEED", $"[{initial_speed},{initial_speed * 2}]");
            template = template.Replace("INITIAL_ROTATION", initial_rotation.ToString());
            template = template.Replace("ROTATION_SPEED", rotation_speed.ToString());
            template = template.Replace("FORCE", force);
            template = template.Replace("FRICTION", friction.ToString());
            template = template.Replace("ALPHA_START", alpha_start.ToString());
            template = template.Replace("ALPHA_END", alpha_end.ToString());
            template = template.Replace("SIZE_START", size_start);
            //template = template.Replace("SIZE_END", size_end.ToString());
            template = template.Replace("FACE_MOVING_DIRECTION", face_moving_direction);
            template = template.Replace("SPEED_SCALE", speed_scale.ToString());
            template += notes;

            using (StreamWriter sw = File.AppendText(Randomizer.saveDir + "data/particles.txt.append"))
            {
                sw.WriteLine(template);
            }
            return particle_name;
        }
    }
}
