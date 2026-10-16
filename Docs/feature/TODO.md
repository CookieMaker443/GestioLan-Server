## Architettura repo
- [ ] Aggiungere un rate limiting, (differenziandolo per l endpoint della AI)
- [ ] implementare una chat con un LLM (potendolo cambiare)(come uno locale con Ollama o eventualente con Claude o OpenAI) e i suoi endopoint


## Categorie
- [ ] Aggiungere una colonna per un valore esadecimale per un colore, da usare poi nel client


## Database
quando il container si genera, deve:
- [ ] eliminare l'utente root, o dargli una password complessa (da valutare)


## Immagini
- [ ] aggiungere controli sul file, verificare che sia un immagine .png, .jpg con i magic bytes


## Utenti
- [ ] Primo utente creato nel database sara root con password root, se nel db venissero eliminati tutti gli utentui, si creerebbe root in automatico
register protetto da jwt
- [ ] aggiungere un controllo in cui verifica che esiste almeno un admin nella lista


## Client
- [ ] il client, quando aggiunge un item, posta l'immagine, col return ottiene id e nome dell'immagine, loassegna all item e posta l'item


## Metadata provider
- [ ] provare a fetchare immagini anche doppo il PUT degli item (con modifiche) 


## Quantity Type
- [ ] Aggiungere una tabella che contiene solo i tipi di quantità degli item (item, chilogrammi, grammi, ecc)