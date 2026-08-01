# Entities

## Bike

Główna encja roweru. Do zastanowienia czy robimy go jako osobnę encja taką meta + encja w stylu StravaBike i link między nimi.
Tak, żeby nie zafiksować się tylko na stravie, a w przyszłości można było dodać łatwo np. Garmin

- Nazwa
- Marka
- Opis
- Typ? (szosa, mtb, ...)
- Notatki
- Cena
- Miejsce zakupu (jakiś autocomplete, ale chyba nie widzielać do osobnej encji)
- Data zakupu
- Inicjalny przebieg, godziny, elev gain 

## Component

Encja komponentów łańcuch, opony, koła, owijki itp. 
Sporo rzeczy wspólnych z Bike - do zastanowienia czy wydzielać jakieś bazowe, ale chyba nie ma sensu?

- Nazwa
- Typ (Kategoria np. Koło, Łańcuch, Pedały, Opony itp.)
- Notatki
- Cena
- Miejsce zakupu (jakiś autocomplete, ale chyba nie widzielać do osobnej encji)
- Data zakupu
- Inicjalny przebieg, godziny, elev gain
- Magazyn gdy nie jest przypisany do roweru innego komponentu, aby było wiadome gdzie jest np. Dom, Garaż, itp. 

## Activities

Pobierane jazdy z stravy. Też do zastanowienia, żeby móżna łatwo dodać z innych serwisów czy nie dodać czegoś w stylu external_service, external_id.

- Data
- Rower
- Dystans
- Czas netto
- Czas brutto
- Wznios
- jakieś maxy | avg | moce?

## Warehose | Store

Tutaj chciałbym, aby do tego można było przypisać nie zamontowane komponenty - np. dom, garaż, szuflax 123 itd.
Do zastanowienia czy dla zużytych zrobić osobny domyślny w stylu Trash, Retried czy jako flaga na komponencie rowerze.

- Nazwa


## Instalation

Przypisanie danego komponentu.

Chciałbym jeszcze móc robić coś jak meta komponenty.
Np. robie komponent Koło i do niego przypisuje, Opona, Kaseta, Tarcza hamulcowa. Taki cały komponent można przekładać między rowerami.
Tutaj chyba jeden poziom zagłebienia, albo nie ograniczać.

Fajnie jakby się dało dodać jako historyczny wpis bez dat itp. tylko np. jaki rower + przebieg (ręcznie) i żeby się to nie psuło.
Use case jest taki, że mam ponotowane, że np. opona jeździła w tym i tym rowerze tyle km, ale już nie mma dat itp, aby sie to samo wyliczyło.


- Rower / Komponent
- Data Od
- Data Do
- Miejsce (tył, przód, lewa, prawa, dół, góra) np. opony przód|tył, łożyska lewe|prawe, stery dół|góra.

## Servis

Możliwość śledzenia kosztów serwisu. Np. naprawa koła.

- Rower / Komponent
- Data
- Cena
- Miejsce serwisu (analogicznie jak miejsce zakupu)

## Accessories / Tools / Pruchases

To ma być na zasadzie jak komponent, ale bez spinania z rowerem.
Bardziej do śledzenia kosztów. Np. narzędzia, smary, chemia, ubrania, buty itp.
Pytanie czy jest to sens wydzielać czy po prostu odpowiednie kategorie w komponencie.
Może nie ma wtedy takie rzeczy będą bez spięcia.

- Nazwa
- Typ
- Notatki
- Cena
- Miejsce zakupu (jakiś autocomplete, ale chyba nie widzielać do osobnej encji)
- Data zakupu
- Magazyn

