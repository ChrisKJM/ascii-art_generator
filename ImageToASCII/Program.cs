using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace ImageToASCII
{
    class Program
    {
        /*
        Wprowadzenie:
        Program jest konsolowy, ze względu na to, iż początkowym założeniem było wyświetlanie obrazu w konsoli.
        Zrezygnowałem z tego, ze względu na możliwe wielkości niektórych obrazów.

        Program zamyka się automatycznie po wygenerowaniu obrazka i zamknięciu MessageBoxa.
        ASCII-art zapisywany jest do pliku image.txt w tym samym folderze co plik exe.

        W notepad++ wyświetla się dobrze, w notepad nie (przynajmniej mi). Innych edytorów tekstu nie testowałem.
        Sprawdzona czcionka: Courier new. Ważne, żeby używana czcionka miała stałą szerokość znaków.
        Te czcionki w teorii powinny działać: https://en.wikipedia.org/wiki/List_of_monospaced_typefaces

        Jeśli ASCII-art jest nie równy, to najprawdopodobniej kwestia czcionki lub zawijania linijek.

        Wyjściowy obrazek może być trochę mniej szeroki niż oryginał, jest tak ze względu na to, iż większość czcionek 
        ma większą wysokość znaków niż ich szerokość.
        */

        // STAThread potrzebne do wykorzystania OpenFileDialog, stwierdziłem że nie ma sensu tworzyć Forma tylko z tego powodu
        [STAThread]
        static void Main(string[] args)
        {
            //Zestawy znaków, jeden standardowy, drugi dla negatywu
            string[] charList = { "@", "#", "$", "%", "&", "?", "<", "=", "+", "!", ";", "-", "*", "^", ",", " " };
            string[] charListAlt = { " ", ",", "^", "*", "-", ";", "!", "+", "=", "<", "?", "&", "%", "$", "#", "@" };
            
            //Dialog odpowiedzialny za wybieranie pliku
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images|*.png;*.jpg;*.jpeg;*.jfif;*.tif;*.tiff;*.bmp";

            //Bitmapa (obrazek)
            Bitmap image = null;

            DialogResult dialogResult = ofd.ShowDialog();

            //Tablica przechowujące kolory
            Color[,] grayscaleColorList;

            //Dokładność obrazka (dokładniej to pierwiastek z ilości px przypadających na jeden znak)
            //1 to najdokładniejszy obrazek, im większa wartość tym mniej dokładny
            int res;

            //Pobieranie pliku
            if (dialogResult == DialogResult.OK)
            {
                image = new Bitmap(ofd.FileName);
                MessageBox.Show("Załadowano plik.");
            } 
            else if (dialogResult == DialogResult.Cancel)
            {
                MessageBox.Show("Nie wybrano pliku.");
                Environment.Exit(0);
            }

            //Pobieram dokładność
            int answer = 1;
            bool correctAnswer = false;
            while (!correctAnswer)
            {
                Console.WriteLine("Podaj dokładność (a za razem rozmiar) obrazka w postaci liczby naturalnej");
                Console.WriteLine("(Im większa liczba tym mniejsza dokładność i mniejszy rozmiar, największa dokładność to 1)");
                string response = Console.ReadLine();
                try
                {
                    answer = int.Parse(response);
                    if (answer < 1) Console.WriteLine("Liczba mniejsza od 1\n");
                    else correctAnswer = true;
                }
                catch
                {
                    Console.WriteLine("Nie podano liczby\n");
                }
            }

            res = answer;

            grayscaleColorList = new Color[(int) Math.Ceiling(image.Width / (float)res),(int)Math.Ceiling(image.Height / (float)res)];

            //Przechodzę przez kolejne grupy pikseli, pobieram ich średni kolor oraz zamieniam ten kolor na czarno-biały
            for (int i = 0; i < image.Width; i += res)
            {
                for (int j = 0; j < image.Height; j += res)
                {
                    int sizeX = res;
                    int sizeY = res;

                    if (i + res > image.Width) sizeX = image.Width - i;
                    if (j + res > image.Height) sizeY = image.Height - j;
                    Color tempColor = GetAveragePixelColor(image, i, j, sizeX, sizeY);
                    int grayscale = (tempColor.R + tempColor.G + tempColor.B) / 3;
                    grayscaleColorList[i / res, j / res] = Color.FromArgb(grayscale, grayscale, grayscale);
                }
            }

            //Co ile odcieni szarości ma być nowy znak
            int charStep = 256 / charList.Length;

            //Usuwanie istniejącego pliku, otwieranie StreamWritera do edycji pliku
            string filePath = Environment.CurrentDirectory + @"\image.txt";
            File.Delete(filePath);
            StreamWriter sw = File.AppendText(filePath);

            //Przechodzę po liście kolorów oraz wpisuję do pliku image.txt odpowiedni znak
            //Znak zależy od ciemności/jasności danego fragmentu obrazka
            for (int i = 0; i < grayscaleColorList.GetLength(1); i++)
            {
                for (int j = 0; j < grayscaleColorList.GetLength(0); j++)
                {
                    sw.Write(charList[grayscaleColorList[j, i].R / charStep]);
                }
                sw.WriteLine();
            }

            MessageBox.Show("Pomyślnie wygenerowano ASCII-art");
        }

        //Zwraca średni kolor pixeli o pozycji między (x, y) oraz (x + sizeX, y + sizeY)
        static private Color GetAveragePixelColor(Bitmap bitmap, int x, int y, int sizeX, int sizeY)
        {
            int rAvg = 0, gAvg = 0, bAvg = 0;

            for (int i = x; i < x + sizeX; i++)
            {
                for (int j = y; j < y + sizeY; j++)
                {
                    Color color = bitmap.GetPixel(i, j);
                    rAvg += color.R;
                    gAvg += color.G;
                    bAvg += color.B;
                }
            }

            int sizeSq = sizeX * sizeY;

            rAvg /= sizeSq;
            gAvg /= sizeSq;
            bAvg /= sizeSq;

            return Color.FromArgb(rAvg, gAvg, bAvg);
        }
    }
}
