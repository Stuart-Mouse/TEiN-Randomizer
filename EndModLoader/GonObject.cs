using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace TEiNRandomizer
{
    enum FieldType
    {
        NULLGON,
        STRING,
        NUMBER,
        OBJECT,
        ARRAY,
        BOOL
    };

    enum MergeMode
    {
        DEFAULT,
        APPEND,
        MERGE,
        OVERWRITE,

        //for numbers:
        ADD,
        MULTIPLY
    };

    class GonException : Exception
    {
        public GonException()
        {

        }

        public GonException(string name)
            : base(String.Format("GON Error", name))
        {

        }
    }

    public partial class GonObject
    {
        // DEFAULT CONSTRUCTOR
        public GonObject()
        {
            Name = "";
            Type = FieldType.NULLGON;
            Int_Data = 0;
            Float_Data = 0;
            Bool_Data = false;
            String_Data = "";
            Children_Array = new List<GonObject>();
            Children_Map = new Dictionary<string, int>();
        }

        // STUFF

        //static const GonObject null_gon;
        //static GonObject non_const_null_gon;
        static string last_accessed_named_field = "";   //used for error reporting when a field is missing,
                                                        //this assumes you don't cache a field then try to access it later
                                                        //as the error report for fields uses this value for its message (to avoid creating and destroying a ton of dummy-objects)
                                                        //this isn't a great or super accurate solution for errors, but it's better than nothing

        //default just throws the string, can be set if you want to avoid exceptions
        //static std::function<void(const std::string&)> ErrorCallback;


        // MEMBERS

        private Dictionary<string, int> Children_Map;
        private List<GonObject> Children_Array;
        private int Int_Data;
        private double Float_Data;
        private bool Bool_Data;
        private string String_Data;
        private string Name;
        private FieldType Type;

        public string GetName() => Name;

        // METHODS

        // Methods for getting char type
        static bool IsWhitespace(char c)
        {
            return c == ' ' || c == '\n' || c == '\r' || c == '\t';
        }
        static bool IsSymbol(char c)
        {
            return c == '=' || c == ',' || c == ':' || c == '{' || c == '}' || c == '[' || c == ']';
        }
        static bool IsIgnoredSymbol(char c)
        {
            return c == '=' || c == ',' || c == ':';
        }

        static void DefaultGonErrorCallback(ref string err)
        {
            throw new GonException(err);
        }

        static List<string> Tokenize(string data)
        {
            List<string> tokens = new List<string>();

            bool inString = false;
            bool inComment = false;
            bool escaped = false;
            string current_token = "";
            for (int i = 0; i < data.Length; i++)
            {
                if (!inString && !inComment)
                {
                    if (IsSymbol(data[i]))
                    {
                        if (current_token != "")
                        {
                            tokens.Add(current_token);
                            current_token = "";
                        }

                        if (!IsIgnoredSymbol(data[i]))
                        {
                            current_token += data[i];
                            tokens.Add(current_token);
                            current_token = "";
                        }
                        continue;
                    }

                    if (IsWhitespace(data[i]))
                    {
                        if (current_token != "")
                        {
                            tokens.Add(current_token);
                            current_token = "";
                        }

                        continue;
                    }

                    if (data[i] == '#')
                    {
                        if (current_token != "")
                        {
                            tokens.Add(current_token);
                            current_token = "";
                        }

                        inComment = true;
                        continue;
                    }
                    if (data[i] == '"')
                    {
                        if (current_token != "")
                        {
                            tokens.Add(current_token);
                            current_token = "";
                        }

                        inString = true;
                        continue;
                    }

                    current_token += data[i];
                }

                if (inString)
                {
                    if (escaped)
                    {
                        if (data[i] == 'n')
                        {
                            current_token += '\n';
                        }
                        else
                        {
                            current_token += data[i];
                        }
                        escaped = false;
                    }
                    else if (data[i] == '\\')
                    {
                        escaped = true;
                    }
                    else if (!escaped && data[i] == '"')
                    {
                        if (current_token != "")
                        {
                            tokens.Add(current_token);
                            current_token = "";
                        }
                        inString = false;
                        continue;
                    }
                    else
                    {
                        current_token += data[i];
                        continue;
                    }
                }

                if (inComment)
                {
                    if (data[i] == '\n')
                    {
                        inComment = false;
                        continue;
                    }
                }
            }

            if (current_token != "") tokens.Add(current_token);

            return tokens;
        }

        static GonObject LoadFromTokens(GonTokenStream Tokens)
        {
            GonObject ret = new GonObject();

            if (Tokens.Peek() == "{")
            {         //read object
                ret.Type = FieldType.OBJECT;

                Tokens.Consume(); //consume '{'
                while (Tokens.Peek() != "}")
                {
                    string name = Tokens.Read();

                    ret.Children_Array.Add(LoadFromTokens(Tokens));
                    ret.Children_Map[name] = (int)ret.Children_Array.Count() - 1;
                    ret.Children_Array[ret.Children_Array.Count() - 1].Name = name;

                    if (Tokens.error)
                    {
                        throw new GonException("GON ERROR: missing a '}' somewhere");
                    }
                }
                Tokens.Consume(); //consume '}'

                return ret;
            }
            else if (Tokens.Peek() == "[")
            {  //read array
                ret.Type = FieldType.ARRAY;

                Tokens.Consume(); //consume '['
                while (Tokens.Peek() != "]")
                {
                    ret.Children_Array.Add(LoadFromTokens(Tokens));

                    if (Tokens.error)
                    {
                        throw new GonException("GON ERROR: missing a ']' somewhere");
                        return null;
                    }
                }
                Tokens.Consume(); //consume ']'

                return ret;
            }
            else
            {
                //read data value
                ret.Type = FieldType.STRING;
                ret.String_Data = Tokens.Read();

                //if string data can be converted to a number, do so
                if (ret.String_Data.All(Char.IsDigit))
                {
                    ret.Int_Data = Convert.ToInt32(ret.String_Data);
                    ret.Type = FieldType.NUMBER;
                }
                /*char* endptr;
                ret.int_data = strtol(ret.string_data.c_str(), &endptr, 0);
                if (*endptr == 0)
                {
                    ret.type = FieldType.NUMBER;
                }*/

                if (IsStringFloat(ret.String_Data))
                {
                    ret.Float_Data = Convert.ToDouble(ret.String_Data);
                    ret.Type = FieldType.NUMBER;
                }
                /*ret.float_data = strtod(ret.string_data.c_str(), &endptr);
                if (*endptr == 0)
                {
                    ret.type = FieldType.NUMBER;
                }*/

                //if string data can be converted to a bool or null, convert
                if (ret.String_Data == "null") ret.Type = FieldType.NULLGON;
                if (ret.String_Data == "true")
                {
                    ret.Type = FieldType.BOOL;
                    ret.Bool_Data = true;
                }
                if (ret.String_Data == "false")
                {
                    ret.Type = FieldType.BOOL;
                    ret.Bool_Data = false;
                }

                return ret;
            }
        }
        static bool IsStringFloat(string str)
        {
            foreach (char c in str)
            {
                if ( (c < '0' || c > '9') && c != '.' )
                    return false;
            }

            return true;
        }

        public static GonObject Load(string filename)
        {
            string buffer = File.ReadAllText(filename);
            return LoadFromBuffer(buffer);
        }

        public static GonObject LoadFromBuffer(string buffer)
        {
            string str = "{"+buffer+"}";
            List<string> Tokens = Tokenize(str);

            GonTokenStream ts = new GonTokenStream();
            ts.current = 0;
            ts.Tokens = Tokens;

            return LoadFromTokens(ts);
        }

        //options with error throwing
        public string String()
        {
            if(Type == FieldType.NULLGON) throw new GonException("GON ERROR: Field \""+(last_accessed_named_field)+"\" does not exist");
            if(Type != FieldType.STRING && Type != FieldType.NUMBER && Type != FieldType.BOOL) throw new GonException("GON ERROR: Field \""+(last_accessed_named_field)+"\" is not a string");
            return String_Data;
        }
        public int Int()
        {
            if(Type == FieldType.NULLGON) throw new GonException("GON ERROR: Field \"" + (last_accessed_named_field) + "\" does not exist");
            if (Type != FieldType.NUMBER) throw new GonException("GON ERROR: Field \"" + (last_accessed_named_field) + "\" is not a number");
            return Int_Data;
        }
        public double Number()
        {
            if(Type == FieldType.NULLGON) throw new GonException("GON ERROR: Field \"" + (last_accessed_named_field) + "\" does not exist");
            if (Type != FieldType.NUMBER) throw new GonException("GON ERROR: Field \"" + (last_accessed_named_field) + "\" is not a number");
            return Float_Data;
        }
        public bool Bool()
        {
            if(Type == FieldType.NULLGON) throw new GonException("GON ERROR: Field \"" + (last_accessed_named_field) + "\" does not exist");
            if (Type != FieldType.BOOL) throw new GonException("GON ERROR: Field \"" + (last_accessed_named_field) + "\" is not a bool");
            return Bool_Data;
        }

        //options with a default value
        public string String(string _default)
            {
            if(Type != FieldType.STRING && Type != FieldType.NUMBER && Type != FieldType.BOOL) return _default;
            return String_Data;
        }

        public int Int(int _default) {
            if(Type != FieldType.NUMBER) return _default;
        return Int_Data;
        }
        public double Number(double _default) {
            if(Type != FieldType.NUMBER) return _default;
        return Float_Data;
        }
        public bool Bool(bool _default) {
            if(Type != FieldType.BOOL) return _default;
        return Bool_Data;
        }

        public bool Contains(string child)
        {
            if(Type != FieldType.OBJECT) return false;
            return Children_Map.ContainsKey(child);
        }
        public bool Contains(int child)
        {
            if(Type != FieldType.OBJECT && Type != FieldType.ARRAY) return true;

            if(child< 0) return false;
            if(child >= Children_Array.Count()) return false;
            return true;
        }
        public bool Exists() { return Type != FieldType.NULLGON; }

        public GonObject ChildOrSelf(string child)
        {
            if(Contains(child)) return (this)[child];
            else return this;
        }
        /*GonObject ChildOrSelf(string child)
        {
            if (Contains(child)) return (this)[child];
            else return this;
        }*/

        public GonObject this[string child]
        {
            get
            {
                last_accessed_named_field = child;

                if (Type == FieldType.NULLGON) return null;
                if (Type != FieldType.OBJECT) return null;

                
                if (Children_Map.TryGetValue(child, out int value))
                {
                    return Children_Array[value];
                }

                return null;
            }
        }

        /*GonObject operator[](string child)
        {
            last_accessed_named_field = child;

            if (type == FieldType::NULLGON) return non_const_null_gon;
            if (type != FieldType::OBJECT) return non_const_null_gon;

            auto iter = children_map.find(child);
            if (iter != children_map.end())
            {
                return children_array[iter->second];
            }

            return null;
        }*/

        public GonObject this[int childindex]
        {
            get
            {
                if (Type != FieldType.OBJECT && Type != FieldType.ARRAY) return this;
                if (childindex < 0 || childindex >= Children_Array.Count()) return null;
                return Children_Array[childindex];
            }
        }
        /*GonObject operator[](int childindex)
        {
            if (type != FieldType::OBJECT && type != FieldType::ARRAY) return * this;
            if (childindex< 0 || childindex >= children_array.size()) return non_const_null_gon;
            return children_array [childindex];
        }*/

        public int Size()
        {
            return size();
        }
        int size()
        {
            if(Type == FieldType.NULLGON) return 0;
            if (Type != FieldType.OBJECT && Type != FieldType.ARRAY) return 1;//size 1, object is self
            return (int)Children_Array.Count();
        }
        public bool empty()
        {
            return (Children_Array.Count() == 0);
        }

        //GonObject begin()
        //{
        //    if (type != FieldType.OBJECT && type != FieldType.ARRAY)
        //        return this;
        //    else return children_array.data();
        //}
        //GonObject end()
        //{
        //    if (type == FieldType.NULLGON)
        //        return this;
        //    if (type != FieldType.OBJECT && type != FieldType.ARRAY)
        //        return this + 1;
        //    else return children_array.data() + children_array.Count();
        //}
        //GonObject begin()
        //{
        //    if (type != FieldType.OBJECT && type != FieldType.ARRAY) return this;
        //    return children_array.data();
        //}
        //GonObject end()
        //{
        //    if (type == FieldType.NULLGON) return this;
        //    if (type != FieldType.OBJECT && type != FieldType.ARRAY) return this + 1;
        //    return children_array.data() + children_array.size();
        //}

        public void DebugOut()
        {
            if (Type == FieldType.OBJECT)
            {
                Console.WriteLine(Name + " is object {");
                for (int i = 0; i < Children_Array.Count(); i++)
                {
                    Children_Array[i].DebugOut();
                }
                Console.WriteLine("}");
            }

            if (Type == FieldType.ARRAY)
            {
                Console.WriteLine(Name + " is array [");
                for (int i = 0; i < Children_Array.Count(); i++)
                {
                    Children_Array[i].DebugOut();
                }
                Console.WriteLine("]");
            }

            if (Type == FieldType.STRING)
            {
                Console.WriteLine(Name + " is string \"" + String() + "\"");
            }

            if (Type == FieldType.NUMBER)
            {
                Console.WriteLine(Name + " is number " + Int() );
            }

            if (Type == FieldType.BOOL)
            {
                Console.WriteLine(Name + " is bool " + Bool() );
            }

            if (Type == FieldType.NULLGON)
            {
                Console.WriteLine(Name + " is null " );
            }
        }

        static string escaped_string(string input)
        {
            var tokenized = Tokenize(input);
            bool needs_quotes = tokenized.Count() != 1 || tokenized[0] != input;

            string out_str = "";
            for (int i = 0; i < input.Count(); i++)
            {
                if (input[i] == '\n')
                {
                    needs_quotes = true;
                    out_str += "\\n";
                }
                else if (input[i] == '\\')
                {
                    needs_quotes = true;
                    out_str += "\\\\";
                }
                else if (input[i] == '\"')
                {
                    needs_quotes = true;
                    out_str += "\\\"";
                }
                else
                {
                    out_str += input[i];
                }
            }

            if (needs_quotes) return "\"" + out_str + "\"";
            return out_str;
        }

        public void Save(string filename)
        {
            using (StreamWriter sw = File.CreateText(filename))
            {
                for (int i = 0; i < Children_Array.Count(); i++)
                {
                    sw.Write(escaped_string(Children_Array[i].Name) + " " + Children_Array[i].GetOutStr() + "\n");
                }
            }
            
        }

        public string GetOutStr(string tab = "", string current_tab = "")
        {
            string strout = "";

            if (Type == FieldType.OBJECT)
            {
                strout += "{\n";
                for (int i = 0; i < Children_Array.Count(); i++)
                {
                    strout += current_tab + tab + escaped_string(Children_Array[i].Name) + " " + Children_Array[i].GetOutStr(tab, tab + current_tab);
                    if (strout.Last() != '\n') strout += "\n";
                }
                strout += current_tab + "}\n";
            }

            if (Type == FieldType.ARRAY)
            {
                bool short_array = true;
                long strlengthtotal = 0;
                for (int i = 0; i < Children_Array.Count(); i++)
                {
                    if (Children_Array[i].Type == FieldType.ARRAY)
                        short_array = false;

                    if (Children_Array[i].Type == FieldType.OBJECT)
                        short_array = false;

                    if (Children_Array[i].Type == FieldType.STRING)
                        strlengthtotal += Children_Array[i].String().Count();


                    if (!short_array) break;
                }
                if (strlengthtotal > 80) short_array = false;

                if (short_array)
                {
                    strout += "[";
                    for (int i = 0; i < Children_Array.Count(); i++)
                    {
                        strout += Children_Array[i].GetOutStr(tab, tab + current_tab);
                        if (i != Children_Array.Count() - 1) strout += " ";
                    }
                    strout += "]\n";
                }
                else
                {
                    strout += "[\n";
                    for (int i = 0; i < Children_Array.Count(); i++)
                    {
                        strout += current_tab + tab + Children_Array[i].GetOutStr(tab, tab + current_tab);
                        if (strout.Last() != '\n') strout += "\n";
                    }
                    strout += current_tab + "]\n";
                }
            }

            if (Type == FieldType.STRING)
            {
                strout += escaped_string(String());
            }

            if (Type == FieldType.NUMBER)
            {
                strout += Int().ToString();
            }

            if (Type == FieldType.BOOL)
            {
                if (Bool())
                {
                    strout += "true";
                }
                else
                {
                    strout += "false";
                }
            }

            if (Type == FieldType.NULLGON)
            {
                strout += "null";
            }

            return strout;
        }


        // COMBINING / PATCHING / MERGING STUFF

        static bool ends_with(ref string str, string suffix)
        {
            if (str.Count() < suffix.Count()) return false;
            for (int i = (int)str.Count() - 1, j = (int)suffix.Count() - 1; j >= 0; j--, i--)
            {
                if (str[i] != suffix[j]) return false;
            }
            return true;
        }
        static void remove_suffix(ref string str, string suffix)
        {
            if (ends_with(ref str, suffix))
            {
                str = str.Remove(str.Last() - suffix.Count(), suffix.Count());
            }
        }

        static MergeMode get_patchmode(ref string str)
        {
            MergeMode policy = MergeMode.DEFAULT;
            if (ends_with(ref str, ".overwrite"))
            {
                policy = MergeMode.OVERWRITE;
            }
            else if (ends_with(ref str, ".append"))
            {
                policy = MergeMode.APPEND;
            }
            else if (ends_with(ref str, ".merge"))
            {
                policy = MergeMode.MERGE;
            }
            else if (ends_with(ref str, ".add"))
            {
                policy = MergeMode.ADD;
            }
            else if (ends_with(ref str, ".multiply"))
            {
                policy = MergeMode.MULTIPLY;
            }
            return policy;
        }

        static string remove_patch_suffixes(ref string str)
        {
            string res = str;
            remove_suffix(ref res, ".overwrite");
            remove_suffix(ref res, ".append");
            remove_suffix(ref res, ".merge");
            remove_suffix(ref res, ".add");
            remove_suffix(ref res, ".multiply");
            return res;
        }

        static void remove_patch_suffixes_recursive(GonObject obj)
        {
            remove_suffix(ref obj.Name, ".overwrite");
            remove_suffix(ref obj.Name, ".append");
            remove_suffix(ref obj.Name, ".merge");
            remove_suffix(ref obj.Name, ".add");
            remove_suffix(ref obj.Name, ".multiply");

            if (obj.Type == FieldType.OBJECT)
            {
                obj.Children_Map.Clear();
                for (int i = 0; i < obj.size(); i++)
                {
                    remove_patch_suffixes_recursive(obj.Children_Array[i]);
                    obj.Children_Map[obj.Children_Array[i].Name] = i;
                }
            }
            else if (obj.Type == FieldType.ARRAY)
            {
                for (int i = 0; i < obj.size(); i++)
                {
                    remove_patch_suffixes_recursive(obj.Children_Array[i]);
                }
            }
        }

        static bool has_patch_suffixes(string str)
        {
            return get_patchmode(ref str) != MergeMode.DEFAULT;
        }


        MergeMode MergePolicyAppend(in GonObject field_a, in GonObject field_b)
        {
            return MergeMode.APPEND;
        }
        MergeMode MergePolicyMerge(in GonObject field_a, in GonObject field_b)
        {
            return MergeMode.MERGE;
        }
        MergeMode MergePolicyOverwrite(in GonObject field_a, in GonObject field_b)
        {
            return MergeMode.OVERWRITE;
        }


        public void InsertChild(GonObject other)
        {
            InsertChild(other.Name, other);
        }
        public void InsertChild(string cname, GonObject other)
        {
            if (Type == FieldType.NULLGON)
            {
                Type = FieldType.OBJECT;
            }

            if (Type == FieldType.OBJECT)
            {
                Children_Array.Add(other);
                Children_Array.Last().Name = cname;
                Children_Map[cname] = (int)Children_Array.Count() - 1;
            }
            else if (Type == FieldType.ARRAY)
            {
                Children_Array.Add(other);
                Children_Array.Last().Name = "";
            }
            else
            {
                new GonException("GON ERROR: Inserting onto incompatible types");
            }
        }

        //void Append(GonObject other)
        //{
        //    if (type == FieldType.NULLGON)
        //    {
        //        this = other;
        //    }
        //    if (type == FieldType.OBJECT && other.type == FieldType.OBJECT)
        //    {
        //        for (int i = 0; i < other.size(); i++)
        //        {
        //            children_array.Add(other[i]);
        //            children_map[other[i].Name] = (int)children_array.Count() - 1;
        //        }
        //    }
        //    else if (type == FieldType.ARRAY && other.type == FieldType.ARRAY)
        //    {
        //        for (int i = 0; i < other.size(); i++)
        //        {
        //            children_array.Add(other[i]);
        //        }
        //    }
        //    else if (type == FieldType.STRING && other.type == FieldType.STRING)
        //    {
        //        string_data += other.string_data;
        //    }
        //    else
        //    {
        //        new GonException("GON ERROR: Append incompatible types");
        //    }
        //}

        //void ShallowMerge(GonObject other, std::function<void(const GonObject& a, const GonObject& b)> OnOverwrite)
        //{
        //    if (type == FieldType.NULLGON)
        //    {
        //        *this = other;
        //    }
        //    else if (type == FieldType::OBJECT && other.type == FieldType::OBJECT)
        //    {
        //        for (int i = 0; i < other.size(); i++)
        //        {
        //            if (Contains(other[i].name))
        //            {
        //                if (OnOverwrite) OnOverwrite((*this)[other[i].name], other[i]);
        //                children_array[children_map[other[i].name]] = other[i];
        //            }
        //            else
        //            {
        //                children_array.push_back(other[i]);
        //                children_map[other[i].name] = (int)children_array.size() - 1;
        //            }
        //        }
        //    }
        //    else if (type == FieldType.ARRAY && other.type == FieldType.ARRAY)
        //    {
        //        for (int i = 0; i < other.size(); i++)
        //        {
        //            children_array.push_back(other[i]);
        //        }
        //    }
        //    else
        //    {
        //        new GonException("GON ERROR: Cannot Shallow Merge incompatible types");
        //    }
        //}


        //void DeepMerge(const GonObject& other, MergePolicyCallback ObjectMergePolicy, MergePolicyCallback ArrayMergePolicy)
        //{
        //    MergeMode policy = ObjectMergePolicy(*this, other);

        //    if (type == FieldType::OBJECT && other.type == FieldType::OBJECT)
        //    {
        //        if (policy == MergeMode::APPEND || policy == MergeMode::ADD)
        //        {
        //            for (int i = 0; i < other.size(); i++)
        //            {
        //                children_array.push_back(other[i]);
        //                children_map[other[i].name] = (int)children_array.size() - 1;
        //            }
        //        }
        //        else if (policy == MergeMode::MERGE || policy == MergeMode::DEFAULT || policy == MergeMode::MULTIPLY)
        //        {
        //            for (int i = 0; i < other.size(); i++)
        //            {
        //                if (Contains(other[i].name))
        //                {
        //                    children_array[children_map[other[i].name]].DeepMerge(other[i], ObjectMergePolicy, ArrayMergePolicy);
        //                }
        //                else
        //                {
        //                    children_array.push_back(other[i]);
        //                    children_map[other[i].name] = (int)children_array.size() - 1;
        //                }
        //            }
        //        }
        //        else if (policy == MergeMode::OVERWRITE)
        //        {
        //            *this = other;
        //        }
        //    }
        //    else if (type == FieldType::ARRAY && other.type == FieldType::ARRAY)
        //    {
        //        if (policy == MergeMode::APPEND || policy == MergeMode::ADD)
        //        {
        //            for (int i = 0; i < other.size(); i++)
        //            {
        //                children_array.push_back(other[i]);
        //            }
        //        }
        //        else if (policy == MergeMode::MERGE || policy == MergeMode::DEFAULT || policy == MergeMode::MULTIPLY)
        //        {
        //            for (int i = 0; i < other.size(); i++)
        //            {
        //                if (i < size())
        //                {
        //                    children_array[i].DeepMerge(other[i], ObjectMergePolicy, ArrayMergePolicy);
        //                }
        //                else
        //                {
        //                    children_array.push_back(other[i]);
        //                }
        //            }
        //        }
        //        else if (policy == MergeMode::OVERWRITE)
        //        {
        //            *this = other;
        //        }
        //    }
        //    else if (type == FieldType::STRING && other.type == FieldType::STRING)
        //    {
        //        if (policy == MergeMode::ADD)
        //        {
        //            string_data += other.string_data;
        //        }
        //        else
        //        {
        //            string_data = other.string_data;
        //        }
        //    }
        //    else if (type == FieldType::NUMBER && other.type == FieldType::NUMBER)
        //    {
        //        if (policy == MergeMode::ADD)
        //        {
        //            float_data += other.float_data;
        //            int_data = int(float_data);
        //            if (float_data == int_data)
        //            {
        //                string_data = std::to_string(int_data);
        //            }
        //            else
        //            {
        //                string_data = std::to_string(float_data);
        //            }
        //            bool_data = float_data != 0;
        //        }
        //        else if (policy == MergeMode::MULTIPLY)
        //        {
        //            float_data *= other.float_data;
        //            int_data = int(float_data);
        //            if (float_data == int_data)
        //            {
        //                string_data = std::to_string(int_data);
        //            }
        //            else
        //            {
        //                string_data = std::to_string(float_data);
        //            }
        //            bool_data = float_data != 0;
        //        }
        //        else
        //        {
        //            float_data = other.float_data;
        //            int_data = other.int_data;
        //            string_data = other.string_data;
        //            bool_data = other.bool_data;
        //        }
        //    }
        //    else
        //    {
        //        *this = other;
        //    }
        //}
    }

    public class GonTokenStream
    {
        public List<string> Tokens = new List<string>();
        public int current;
        public bool error;

        public GonTokenStream()
        {
            current = 0;
            error = false;
        }

        public string Read()
        {
            if (current >= Tokens.Count())
            {
                error = true;
                return "!";
            }

            return Tokens[current++];
        }
        public string Peek()
        {
            if (current >= Tokens.Count())
            {
                error = true;
                return "!";
            }
            return Tokens[current];
        }

        public void Consume()
        { 
            //avoid anonymous string creation/destruction
            if (current >= Tokens.Count())
            {
                error = true;
                return;
            }
            current++;
        }
    };
}
