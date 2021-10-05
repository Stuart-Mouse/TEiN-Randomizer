using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Xml.Linq;


namespace TEiNRandomizer
{
    public class PiecePool : IComparable<PiecePool>, INotifyPropertyChanged
    {
        public string Name { get; set; }
        public List<LevelPiece> Pieces { get; set; }
        public string NumPieces { get; set; }
        public short Order { get; set; }
        public string Folder { get; set; }
        public string Author { get; set; }
        public string Source { get; set; }
        private bool _active { get; set; }
        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;
                OnPropertyChanged(nameof(Active));
            }
        }

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public int CompareTo(PiecePool other) => Order.CompareTo(other.Order);

        public void Save()
        {
            try
            {
                var doc = XDocument.Load($"data/piecepools/{Name}.xml");    // open levelpool file
                doc.Root.Attribute("enabled").Value = Active.ToString();
                doc.Save($"data/levelpools/{Name}.xml");
            }
            catch (Exception)
            {
                MessageBox.Show(
                        "Error saving the piece pool bool.",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning,
                        MessageBoxResult.OK
                    );
                throw;
            }

        }


        public static PiecePool LoadPiecePool(string fileName, string folder)
        {
            PiecePool pool = new PiecePool();
            pool.Name = fileName;
            //pool.Folder = folder;
            pool.Pieces = new List<LevelPiece>();

            var doc = XDocument.Load($"data/piecepools/{pool.Name}.xml");    // open levelpool file
            pool.Active = Convert.ToBoolean(doc.Root.Attribute("enabled").Value == "True");
            pool.Order = Convert.ToInt16(doc.Root.Attribute("order").Value);
            pool.Author = (doc.Root.Attribute("author").Value);
            pool.Source = doc.Root.Attribute("source").Value;
            foreach (var element in doc.Root.Elements())
            {
                if (element.Name == "piece")
                {
                    var piece = new LevelPiece { };
                    piece.Name = element.Attribute("name").Value;
                    piece.Folder = pool.Name;
                    piece.File = LevelManip.Load($"data/levelpieces/{piece.Folder}/{piece.Name}.lvl");
                    piece.CeilingEn = Convert.ToBoolean(element.Attribute("ceilingEn").Value);
                    piece.CeilingEx = Convert.ToBoolean(element.Attribute("ceilingEx").Value);
                    piece.FloorEn   = Convert.ToBoolean(element.Attribute("floorEn").Value);
                    piece.FloorEx   = Convert.ToBoolean(element.Attribute("floorEx").Value);

                    if (element.Attribute("marginTop") != null)
                        piece.Margin.Top = Convert.ToInt32(element.Attribute("marginTop").Value);
                    if (element.Attribute("marginBottom") != null)
                        piece.Margin.Bottom = Convert.ToInt32(element.Attribute("marginTop").Value);

                    pool.Pieces.Add(piece);
                }
            }
            pool.NumPieces = pool.Pieces.Count.ToString() + " pieces";
            return pool;
        }
    }
}