## Architettura repo
- [X] Refactorare la repo e l architettura del codice seguendo la mappa lavorata assieme a Claude per una migliore struttura e ciomprensione
- [X] aggiungere le interfacce come "layer" tra le classi controller e le API che riceveranno con DI (Dependency Injection) l'oggetto interfaccia (implementata dal con troller) come service da Program.cs per permettere dei test con controller (che non toccano il db ma usano l'interfaccia)
- [X] Aggiungere un rate limiting, (differenziandolo per l endpoint della AI)

## Categorie
- [X] Se un user toglie una categoria, il db trova gli item che hanno quella categoria, e toglie il bit di quella cat da essi
- [X] Aggiungere una colonna per un valore esadecimale per un colore, da usare poi nel client

## Database
- [X] importare SOLO i dati nella tabella delle migrazioni
*i dati sono opzionali*

## Docker compose
- [X] Migliorare la sicurezza dei container
- [X] aggiornare i docker compose build per fare il bind mount
- [X] aggiornare il docker compose che pulla le immagini, a pullare l immagine del database gia pronto (ancora da creare il container e da pubblicare)

## Immagini
- [X] creare in src/GestioLan.API/Plugins un interfaccia per crreare pi dei puligin per i vari provider esterni di immagini per le varie categorie usando il pattern `plugin`
- [X] fare una migazione nella tabella category, aggiungere uno (o piu, da vedere) elementi NULLABILI che sono i link delle api che fungeranno da provider per quegli item (Aggiunto una stringa)
- [X] se l'item ha un valore in "barcode" e ha una categoria associata ad un endpoint API per l'immagine, e l'utente manda un FLAG "TryFetchImage" (con o senza immagine dall'utente), il controller prova a fetchare l'immagine dall API
(implementato conun booleano come variabile d'ambiente, che da prioprita alle immagini dell'utente  o delle api)

## Items
- [X] fare refactor degli endpoint dove possibile
- [X] creare i test in Items_test.http
- [X] fare una migrazione e aggiungere una colonna nullabile per dei barcode

## Server
- [X] Aggiungere i log, chi aggiunge cosa, chi legge cosa, chi prova a fare cio che non puo (non admin actions)

## Metadata provider
- [X] a plugin stabili, provare a inserire metodi per fetchare nome, descrizione, se i plugin non possono provvedere , fanno return null


## Quantity Type
- [X] Aggiungere una tabella che contiene solo i tipi di quantità degli item (item, chilogrammi, grammi, ecc)