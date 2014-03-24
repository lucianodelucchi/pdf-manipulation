using iTextSharp.text.pdf;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            RemoveObjects();
        }

        private static void RemoveObjects()
        {
            string file = @"PATH TO THE ORIGINAL PDF";

            PdfReader reader = new PdfReader(file);

            List<PRStream> toDelete = new List<PRStream>();

            //for (int i = 1; i <= reader.NumberOfPages; i++)
            //{
            //    var pdfDictionary = reader.GetPageN(i).GetAsDict(PdfName.RESOURCES);

            //    toDelete.AddRange(FindReferencesToDelete(pdfDictionary, reader));
            //}

            //toDelete.AsParallel().ForAll(stream => stream.SetData(new byte[0]));
            
            /*
             * all the above is not necessary if you know that the object you want to remove is being referenced by the same object
             * eg:
             *  ID_TO_REMOVE_2 (references)-> ID_TO_REMOVE_1
             * 
             * That relationship is being used in all the pages, so there is no point in traversing the whole PDF and 
             * getting all the intermediary objects beacause they're going to be the same!
            */

            //Retrieve the object by ID
            var toDeleteImage = (PRStream)reader.GetPdfObject(ID_TO_REMOVE_1);
            toDeleteImage.SetData(new byte[0]);

            var toDeleteAn = (PRStream)reader.GetPdfObject(ID_TO_REMOVE_2);
            toDeleteAn.SetData(new byte[0]);

            var toDeleteWM = (PRStream)reader.GetPdfObject(ID_TO_REMOVE_3);
            toDeleteWM.SetData(new byte[0]);

            reader.RemoveUnusedObjects();

            using (Stream s = new MemoryStream())
            {
                // Save the changes made back to PDF. 
                PdfStamper stp = new PdfStamper(reader, s);
                stp.Close();

                // Save PDF. 
                File.WriteAllBytes(@"PATH TO SAVE THE MODIFIED PDF", ((MemoryStream)s).ToArray());
            }

            reader.Close();
        }

        private static IEnumerable<PRStream> FindReferencesToDelete(PdfDictionary resource, PdfReader reader)
        {
            var list = new List<PRStream>();

            if (resource == null)
            {
                return list;
            }

            PdfDictionary xObjects = resource.GetAsDict(PdfName.XOBJECT);

            if (xObjects != null)
            {
                foreach (PdfName key in xObjects.Keys)
                {
                    var obj = xObjects.GetAsIndirectObject(key);

                    var pdfObjectDictionary = (PdfDictionary)PdfReader.GetPdfObject(obj);

                    var pdfObjectResources = pdfObjectDictionary.GetAsDict(PdfName.RESOURCES);

                    //change the following to the correct obejct ID, use http://sourceforge.net/projects/itextrups/
                    //to look for the object you want or write your own solution to find the ID you want
                    var idToRemove1 = 0;

                    if (HasDirectDescendant(pdfObjectResources, idToRemove1))
                    {
                        list.Add((PRStream)reader.GetPdfObject(obj.Number));
                    }

                    list.AddRange(FindReferencesToDelete(pdfObjectResources, reader));
                }
            }

            return list;
        }

        private static bool HasDirectDescendant(PdfDictionary resource, int p)
        {
            if (resource == null)
            {
                return false;
            }

            bool isDirectDescendant = false;

            PdfDictionary xObjects = resource.GetAsDict(PdfName.XOBJECT);

            if (xObjects != null)
            {
                foreach (PdfName key in xObjects.Keys)
                {
                    var obj = xObjects.GetAsIndirectObject(key);

                    isDirectDescendant |= obj.Number == p;
                }
            }

            return isDirectDescendant;

        }
    }
}
