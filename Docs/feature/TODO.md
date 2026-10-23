## Architettura repo
- [ ] implementare una chat con un LLM (potendolo cambiare)(come uno locale con Ollama o eventualente con Claude o OpenAI) e i suoi endopoint


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

