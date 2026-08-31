# Bee Strategy - Game Design Document (GDD)

## 🐝 Ogólna Wizja Gry
* **Polityczno-gospodarcza strategia oparta na pszczołach.** Gra nie ma być tylko prostym zbieraniem surowców – ma być rozbudowanym symulatorem społeczeństwa i ekonomii ula.
* Będziesz musiał budować infrastrukturę, m.in. **szkoły** dla pszczół, budynki produkcyjne.
* Złożone mechaniki: zadowolenie ula, bunty, dezercje i kary za dezercję.

## 📜 Księga Praw i Ustroje Polityczne
* Główną mechaniką zarządzania państwem jest **Księga Praw**. Gracz wybiera polityki na zasadzie: `Tak` / `Nie` / `Nieuregulowane`.
* **Przykładowe prawa:** 
  * Darmowa edukacja
  * Wojskowi też zbierają pyłek (Military Gathers Pollen)
  * Państwowa ziemia
* Na podstawie wyborów ul otrzymuje **"łatkę" ustroju** (np. Demokracja, Komunizm, Faszyzm, Totalitaryzm), co odblokowuje premie i specjalne prawa.
* **Konsekwencje polityczne:**
  * *Komunizm:* Ziemia jest państwowa w każdym ulu, brak opłat za wykupywanie terenu.
  * *Totalitaryzm / Faszyzm:* Ogromny wyzysk (budowniczowie pracują w polu). Strach hamuje bunty pszczół.
  * *Demokracja:* Wysyłanie młodych na wojnę drastycznie psuje zadowolenie; społeczeństwo silniej reaguje na błędy władzy.

## 💰 Ekonomia i Surowce (Złota Trójca)
Trzy główne filary ekonomii:

1. **PYŁEK (Pollen) = Pieniądz / Waluta**
   * Służy do rozmnażania (początkowy kapitał) oraz jako **Labour Cost** (wypłaty dla robotników).
   * Rodzaje pyłku (Słonecznik, Tulipan, Mniszek) działają jak odrębne waluty.
   * Planowany system **inflacji i deflacji**. AI mają własne gospodarki rynkowe i preferencje walutowe.

2. **NEKTAR (Nectar) = Surowiec Bazowy (Przemysłowy)**
   * Zbierany na mapie przez Skautów/Zbieraczy, a następnie przerabiany w Ulu przez robotników (Conversion Workers).
   * Gracz zarządza suwakiem produkcji, decydując jaka część nektaru jest zamieniana na Wosk, a jaka na Miód.

3. **WOSK (Wax) i MIÓD (Honey) = Efekt Produkcji**
   * **Wosk:** Materiał (Material Cost) do budowy uli i infrastruktury.
   * **Miód:** Pożywienie niezbędne do utrzymania populacji.

## 🌸 Asymetria Kwiatów
Kwiaty na mapie mają zróżnicowany balans, zmuszając do decyzji strategicznych:
* **Mniszek lekarski (Dandelion):** Mnóstwo Nektaru (potencjał na wosk/miód), mało Pyłku (słaby dochód pieniężny).
* **Słonecznik (Sunflower):** Gigantyczne ilości Pyłku (bogactwo walutowe), mało Nektaru.
* **Tulipan (Tulip):** Zbalansowany / Inne proporcje.

## 👑 Mechanika Stolicy i Rozmnażania
* **Ul Stolica (Capital Hive):** Przebywa w nim Królowa. Ze względu na jej obecność, koszt produkcji nowych pszczół jest zredukowany o 50% (np. 25 nektaru).
* **Ule Zewnętrzne (Outposts):** Jaja/poczwarki muszą być do nich przenoszone przez robotnice. Koszt produkcji pszczół wynosi 100% (np. 50 nektaru).
* UI pozwala zarządzać w każdym ulu tempem produkcji (suwak Pszczoły / 5 minut), pozwalając na elastyczne przenoszenie zasobów ludzkich.
