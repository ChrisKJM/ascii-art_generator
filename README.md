# ascii-art_generator
Creates ASCII art from images.

Program jest konsolowy, ze względu na to, iż początkowym założeniem było wyświetlanie obrazu w konsoli.
Zrezygnowałem z tego, ze względu na możliwe wielkości niektórych obrazów.

Program zamyka się automatycznie po wygenerowaniu obrazka i zamknięciu MessageBoxa.
ASCII-art zapisywany jest do pliku image.txt w tym samym folderze co plik exe.

W notepad++ wyświetla się dobrze, w notepad nie (przynajmniej mi). Innych edytorów tekstu nie testowałem.
Sprawdzona czcionka: Courier new. Ważne, żeby używana czcionka miała stałą szerokość znaków.

Jeśli ASCII-art jest nie równy, to najprawdopodobniej kwestia czcionki lub zawijania linijek.

Wyjściowy obrazek może być trochę mniej szeroki niż oryginał, jest tak ze względu na to, iż większość czcionek 
ma większą wysokość znaków niż ich szerokość.
