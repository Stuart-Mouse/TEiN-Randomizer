using System.Collections.Generic;
using System.IO;

namespace TEiNRandomizer
{
    public static partial class Randomizer
    {

        public struct WorldMapFile
        {
            // meta
            public string default_node;

            public string leftwipes;
            public string rightwipes;
            public string upwipes;
            public string downwipes;

            public string stevenheads;
            public string steventhreshtumors;
            public string stevenlevels;

            public string mainworlds;
            public string cartworlds;

            public string headlevel;
            public string bodylevel;
            public string heartlevel;

            public string mute_warpunlock_message;
            public string game_over_checkpoints;
            public string iron_cart_entrypoints;
            public string timed_cart_entrypoints;
            public string console_entry_levels;

            public string nevermore_entry;
            public string escape_entry;
            public int    nevermore_tumorthresh;
            public string timer_bypass_levels;
            public string timer_buttonopen_levels;

            public string megacart_entry_levels;
            public string towercart_entry_levels;
            public string cart_lives_overrides;
            public string cart_title_overrides;

            public int    savefile_completion_counter_max;
            public string savefile_completion_warpoints;
            public string save_initiallevel;
            public string save_lightspawn;
            public string save_darkspawn;
            public string save_lightspawnlabel;
            public string save_darkspawnlabel;

            // pages (not yet implemented)
            // List<string> pages;

            // nodes (not implemented yet)
            public List<Node> Nodes;
            public struct Node
            {
                public string id;

                public string clipname;
                public string entrylevel;
                public string page;
                public string levelgroups;
                public string unlock_condition;
                public string unlock_condition_levels;
                public string tumor_threshold;

                public string select;
                public string up;
                public string down;
                public string left;
                public string right;

                public string cart_cheevolevel;
                public string cart_splashframe;

                public void Write(StreamWriter sw)
                {
                    sw.WriteLine($"{id} {{");

                    if (clipname != null) sw.WriteLine($"clipname {clipname}");
                    if (entrylevel != null) sw.WriteLine($"entrylevel {entrylevel}");
                    if (page != null) sw.WriteLine($"page {page}");
                    if (levelgroups != null) sw.WriteLine($"levelgroups {levelgroups}");

                    if (unlock_condition != null) sw.WriteLine($"unlock_condition {unlock_condition}");
                    if (unlock_condition_levels != null) sw.WriteLine($"unlock_condition_levels {unlock_condition_levels}");
                    if (tumor_threshold != null) sw.WriteLine($"tumor_threshold {tumor_threshold}");

                    if (select != null) sw.WriteLine($"select {select}");
                    if (up != null) sw.WriteLine($"up {up}");
                    if (down != null) sw.WriteLine($"down {down}");
                    if (left != null) sw.WriteLine($"left {left}");
                    if (right != null) sw.WriteLine($"right {right}");

                    if (cart_cheevolevel != null) sw.WriteLine($"cart_cheevolevel {cart_cheevolevel}");
                    if (cart_splashframe != null) sw.WriteLine($"cart_splashframe {cart_splashframe}");

                    sw.WriteLine("}");
                }
            }

            public void Write()
            {
                StreamWriter sw = new StreamWriter($"{SaveDir}/data/worldmap.txt");

                sw.WriteLine("pages {");
                //foreach (var page in pages)
                //    sw.WriteLine(page);
                sw.WriteLine("lightworld { collectibles 238 }");
                sw.WriteLine("darkworld { collectibles 7 }");
                sw.WriteLine("console { collectibles 0 }");
                sw.WriteLine("}");

                sw.WriteLine("meta {");
                sw.WriteLine($"default_node {default_node}");

                sw.WriteLine($"leftwipes [{leftwipes}]");
                sw.WriteLine($"rightwipes [{rightwipes}]");
                sw.WriteLine($"upwipes [{upwipes}]");
                sw.WriteLine($"downwipes [{downwipes}]");

                sw.WriteLine($"stevenheads [{stevenheads}]");
                sw.WriteLine($"steventhreshtumors [{steventhreshtumors}]");
                sw.WriteLine($"stevenlevels [{stevenlevels}]");

                sw.WriteLine($"mainworlds [{mainworlds}]");
                sw.WriteLine($"cartworlds [{cartworlds}]");

                sw.WriteLine($"headlevel {headlevel}");
                sw.WriteLine($"bodylevel {bodylevel}");
                sw.WriteLine($"heartlevel {heartlevel}");

                sw.WriteLine($"mute_warpunlock_message [{mute_warpunlock_message}]");
                sw.WriteLine($"game_over_checkpoints [{game_over_checkpoints}]");
                sw.WriteLine($"iron_cart_entrypoints [{iron_cart_entrypoints}]");
                sw.WriteLine($"timed_cart_entrypoints [{timed_cart_entrypoints}]");
                sw.WriteLine($"console_entry_levels [{console_entry_levels}]");

                sw.WriteLine($"nevermore_entry {nevermore_entry}");
                sw.WriteLine($"escape_entry {escape_entry}");
                sw.WriteLine($"nevermore_tumorthresh {nevermore_tumorthresh}");
                sw.WriteLine($"timer_bypass_levels [{timer_bypass_levels}]");
                sw.WriteLine($"timer_buttonopen_levels [{timer_buttonopen_levels}]");

                sw.WriteLine($"megacart_entry_levels [{megacart_entry_levels}]");
                sw.WriteLine($"towercart_entry_levels [{towercart_entry_levels}]");
                sw.WriteLine($"cart_lives_overrides [{cart_lives_overrides}]");
                sw.WriteLine($"cart_title_overrides [{cart_title_overrides}]");

                sw.WriteLine($"savefile_completion_counter_max {savefile_completion_counter_max}");
                sw.WriteLine($"savefile_completion_warpoints [{savefile_completion_warpoints}]");
                sw.WriteLine($"save_initiallevel {save_initiallevel}");
                sw.WriteLine($"save_lightspawn {save_lightspawn}");
                sw.WriteLine($"save_darkspawn {save_darkspawn}");
                sw.WriteLine($"save_lightspawnlabel \"{save_lightspawnlabel}\"");
                sw.WriteLine($"save_darkspawnlabel \"{save_darkspawnlabel}\"");
                sw.WriteLine("}");

                sw.WriteLine("nodes {");
                //foreach (Node node in Nodes)
                //    node.Write(sw);
                string nodes = System.IO.File.ReadAllText("data/text/worldmap_nodes.txt");
                sw.WriteLine(nodes);
                sw.WriteLine("}");
                sw.Close();
            }
            public void Init()
            {
                default_node = "TheEnd";

                leftwipes = "";
                rightwipes = "";
                upwipes = "";
                downwipes = "";

                stevenheads = "";
                steventhreshtumors = "";
                stevenlevels = "";

                mainworlds = "";
                cartworlds = "";

                headlevel = "none";
                bodylevel = "none";
                heartlevel = "none";

                mute_warpunlock_message = "";

                game_over_checkpoints = "";
                iron_cart_entrypoints = "";
                timed_cart_entrypoints = "";
                console_entry_levels = "";
                nevermore_entry = "7-0";
                escape_entry = "6-21";
                nevermore_tumorthresh = 450;
                timer_bypass_levels = "";
                timer_buttonopen_levels = "";

                megacart_entry_levels = "";
                towercart_entry_levels = "";
                cart_lives_overrides = "";
                cart_title_overrides = "";

                savefile_completion_counter_max = 1;
                savefile_completion_warpoints = "";
                save_initiallevel = "hub-1";
                save_lightspawn = "hub-1";
                save_darkspawn = "hubd-1";
                save_lightspawnlabel = "The End";
                save_darkspawnlabel = "Anguish";
            }
        }
    }
}
