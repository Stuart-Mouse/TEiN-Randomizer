using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace TEiNRandomizer
{
    public static class TilesetManip
    {
        // CONSTRUCTOR
        static TilesetManip()
        {
            // Load Resources
            var gon = GonObject.Load("data/text/tilesets_pools.gon");
            NumPalettes         = gon["palettes"        ].Int();
            TileGraphicsPool    = gon["tile_graphics"   ].ToStringArray();
            OverlayGraphicsPool = gon["overlay_graphics"].ToStringArray();
            ParticlePool        = gon["particles"       ].ToStringArray();
            MusicPool           = gon["music"           ].ToStringArray();
            ArtAlts             = GonObject.Load("data/text/art_alts.gon");
        }

        // MEMBERS

        // Pools for randomized tileset properties
        static string[] TileGraphicsPool { get; set; }
        static string[] OverlayGraphicsPool { get; set; }
        static string[] BackgroundPool { get; set; }
        static string[] ParticlePool { get; set; }
        static string[] MusicPool { get; set; }
        static GonObject ArtAlts { get; set; }
        public static int NumPalettes { get; set; }
        static List<Shader> ShaderPool { get; set; }
        static string[] AreaTypes { get; set; } = AppResources.AreaTypes;   // References area types from app resources

        // METHODS

        // Shader pool is re-evaluated each time the randomization process begins
        public static void MakeShaderPool()
        {
            ShaderPool = new List<Shader> { };

            foreach (var shader in AppResources.ShadersList)
            {
                if (shader.Enabled)
                    ShaderPool.Add(shader);
            }
        }

        // Functions for getting random values
        public static int GetPalette() { return RNG.random.Next(1, NumPalettes); }
        public static string GetTile() { return TileGraphicsPool[RNG.random.Next(0, TileGraphicsPool.Count())]; }
        public static string GetMusic() { return MusicPool[RNG.random.Next(0, MusicPool.Count())]; }
        public static string GetOverlay() { return OverlayGraphicsPool[RNG.random.Next(0, OverlayGraphicsPool.Count())]; }
        public static Dictionary<string, string> GetArtAlts()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            switch (AppResources.MainSettings.AltLevel)
            {

            }
            if (AppResources.MainSettings.AltLevel == "Safe")
            {
                for (int i = 0; i < ArtAlts["safe"].Size(); i++)
                {
                    var art = ArtAlts["safe"][i];
                    var alts = art.ToStringArray();
                    ret.Add(art.GetName(), alts[RNG.random.Next(0, alts.Length)].Trim());
                }
            }
            else if (AppResources.MainSettings.AltLevel == "Extended")
            {
                for (int i = 0; i < ArtAlts["extended"].Size(); i++)
                {
                    var art = ArtAlts["extended"][i];
                    var alts = art.ToStringArray();
                    ret.Add(art.GetName(), alts[RNG.random.Next(0, alts.Length)].Trim());
                }
            }
            else if (AppResources.MainSettings.AltLevel == "Crazy")
            {
                for (int i = 0; i < ArtAlts["crazy"].Size(); i++)
                {
                    var art = ArtAlts["crazy"][i];
                    var alts = art.ToStringArray();
                    ret.Add(art.GetName(), alts[RNG.random.Next(0, alts.Length)].Trim());
                }
            }
            else if (AppResources.MainSettings.AltLevel == "Insane")
            {
                var art = ArtAlts["insane"];
                for (int i = 0; i < art.Size(); i++)
                {
                    var alts = ArtAlts["insane"].ToStringArray();
                    ret.Add(art[i].String(), alts[RNG.random.Next(0, alts.Length)].Trim());
                }
            }

            return ret;
        }
        public static Tileset GetTileset()
        {
            // Create new tileset to return
            Tileset tileset = new Tileset();

            // Set tileset info
            tileset.tile_graphics = GetTile();
            tileset.overlay_graphics = GetOverlay();
            tileset.palette = GetPalette();
            tileset.music = GetMusic();

            // Set fx_shader_mid info
            if (AppResources.MainSettings.DoShaders && ShaderPool.Count() > 0)
            {
                if (RNG.random.Next(0, 2) == 0)
                {
                    Shader shader_mid = ShaderPool[RNG.random.Next(0, ShaderPool.Count())];
                    tileset.fx_shader_mid  = shader_mid.fx_shader_mid;
                    tileset.midfx_graphics = shader_mid.midfx_graphics;
                    tileset.midfx_layer    = shader_mid.midfx_layer;
                    tileset.shader_param   = shader_mid.shader_param;
                }
                if (tileset.fx_shader_mid != null)
                    tileset.shader_param = $"0.{RNG.random.Next(10, 100)}";
            }

            // Select or generate particle effects
            if (AppResources.MainSettings.DoParticles)
            {
                // Particles are generated where necessary
                if (AppResources.MainSettings.GenerateCustomParticles)
                {
                    // Select the number of particles to generate, based on the max particles setting
                    int count = RNG.random.Next(0, AppResources.MainSettings.MaxParticleEffects);

                    // Switch case "fallthrough" allows use to generate the proper number particles in descending order (This is retarded????)
                    switch (count)
                    {
                        case 2:
                            tileset.global_particle_3 = ParticleGenerator.GetParticle();
                            goto case 1;
                        case 1:
                            tileset.global_particle_2 = ParticleGenerator.GetParticle();
                            goto case 0;
                        case 0:
                            tileset.global_particle_1 = ParticleGenerator.GetParticle();
                            break;
                    }
                }
                // If we aren't generating particles, select from the standard pool
                else
                {
                    // Same principle as above for this switch case
                    int count = RNG.random.Next(0, AppResources.MainSettings.MaxParticleEffects);
                    switch (count)
                    {
                        case 2:
                            tileset.global_particle_3 = ParticlePool[RNG.random.Next(0, ParticlePool.Count())];
                            goto case 1;
                        case 1:
                            tileset.global_particle_2 = ParticlePool[RNG.random.Next(0, ParticlePool.Count())];
                            goto case 0;
                        case 0:
                            tileset.global_particle_1 = ParticlePool[RNG.random.Next(0, ParticlePool.Count())];
                            break;
                    }
                }
            }

            // Generate art alts
            if (AppResources.MainSettings.AltLevel != "None")
                tileset.art_alts = GetArtAlts();

            // Set tilt and wobble
            if (AppResources.MainSettings.DoNevermoreTilt && RNG.random.Next(0, 6) == 0)
            {
                tileset.do_tilt = true;
            }
            if (AppResources.MainSettings.DoExodusWobble && RNG.random.Next(0, 6) == 0)
            {
                tileset.do_wobble = true;
            }

            // Generate physics
            if (AppResources.MainSettings.DoPhysics)
            {
                if (AppResources.MainSettings.PlatformPhysics)
                    tileset.platform_physics = Physics.PlatformPhysics();

                if (AppResources.MainSettings.WaterPhysics)
                    tileset.water_physics    = Physics.WaterPhysics();

                if (AppResources.MainSettings.PlayerPhysics)
                    tileset.player_physics   = Physics.PlayerPhysics();

                if (AppResources.MainSettings.LowGravPhysics)
                    tileset.lowgrav_physics  = Physics.LowGravPhysics();
            }

            return tileset;
        }
    }
}
