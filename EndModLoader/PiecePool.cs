using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace TEiNRandomizer
{
    public class PiecePool : IComparable<PiecePool>
    {
        public string Name { get; set; }
        public List<LevelPiece> Pieces { get; set; }
        public bool Active { get; set; }
        public string NumPieces { get; set; }
        public short Order { get; set; }
        public string Folder { get; set; }
        public string Author { get; set; }
        public string Source { get; set; }

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


        public PiecePool(string fileName, string folder)
        {
            this.Name = fileName;
            //this.Folder = folder;
            this.Pieces = new List<LevelPiece>();

            var doc = XDocument.Load($"data/piecepools/{Name}.xml");    // open levelpool file
            this.Active = Convert.ToBoolean(doc.Root.Attribute("enabled").Value == "True");
            this.Order = Convert.ToInt16(doc.Root.Attribute("order").Value);
            this.Author = (doc.Root.Attribute("author").Value);
            this.Source = doc.Root.Attribute("source").Value;
            foreach (var element in doc.Root.Elements())
            {
                if (element.Name == "piece")
                {
                    var piece = new LevelPiece { };
                    piece.Name = element.Attribute("name").Value;
                    piece.Folder = this.Name;
                    piece.File = LevelManip.Load($"data/levelpieces/{piece.Folder}/{piece.Name}.lvl");

                    this.Pieces.Add(piece);
                }
            }
            NumPieces = this.Pieces.Count.ToString() + " pieces";
        }
    }
}