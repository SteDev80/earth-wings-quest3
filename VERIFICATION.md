# Verifica del prototipo

- Compilazione C# e preparazione della scena: completate con Unity 6000.6.0f1 e Cesium 1.25.1.
- Build Android IL2CPP: completata; `Builds/EarthWings.apk`.
- Manifest APK controllato con Android aapt: categoria `com.oculus.intent.category.VR`, head tracking VR, Quest 3 (`eureka`) fra i dispositivi supportati.
- Architettura del pacchetto: solo `arm64-v8a`.
- Firma APK verificata con apksigner: valida, schema v2.
- Librerie native presenti: Cesium, Unity OpenXR, OpenXR loader, IL2CPP.
- Impostazioni persistenti: Meta Quest Support e Oculus Touch Controller Profile abilitati; Input System attivo.
- Prototipo installato su Quest 3 tramite ADB. L'utente ha confermato la visualizzazione dell'area di addestramento e la variazione dell'indicatore Ala con i controller.
- Account Cesium configurato con token dedicato alla sola lettura dell'asset Google Photorealistic 3D Tiles (2275207). Valore del token escluso da Git.
- Verificati accesso autenticato all'asset e ricezione del tileset radice Google 3D Tiles 1.0.
- APK con mappe ricompilato e installato. Partenza configurata a Roma (41.8902, 12.4922), quota locale 1000 m.
- Superata la richiesta di attivazione dei controller: processo dell'app in esecuzione sul Quest, log con elaborazione delle mesh cartografiche. Unity segnala triangoli di grandi dimensioni nelle mesh di collisione dei tile: stabilità delle collisioni da verificare.
- L'utente ha confermato di vedere Roma. Ha segnalato sdoppiamento/cambiamento dell'immagine muovendo la testa.
- Correzione in verifica: la camera usa TrackedPoseDriver con sorgente Center Eye e UpdateAndBeforeRender; rimossa la scrittura manuale della posa XRNode.Head dal controller di volo. Esito visivo ancora da confermare sul Quest.
- Attribuzioni VR e fluidità ancora da verificare con l'utente.
- L'utente ha precisato che il disallineamento riguarda soltanto Roma, mentre scritte e controller sono allineati. Preparata e compilata una versione Android con OpenXR MultiPass (un passaggio per occhio) e matrici stereo gestite dal runtime. La correzione visiva richiede conferma sul dispositivo; MultiPass può aumentare il costo del rendering.
- L'utente ha confermato la risoluzione del disallineamento con MultiPass.
- Aggiunti turbo x2 tenendo X sul controller sinistro e salita progressiva guardando in alto. Compilazione APK e verifica firma completate. Alla ripresa del 7 settembre ADB non rileva dispositivi: installazione di questo aggiornamento e prova dei nuovi comandi ancora da completare.
- Aggiunta partenza sopra Courmayeur (45.7874, 6.9731) a quota locale 3200 m dall'origine geografica a quota zero. Y alterna Courmayeur e Roma, riportando il volo in pausa. Copertura e caricamento Google nella nuova area ancora da verificare sul Quest.
- Aggiornamento Courmayeur/turbo/salita installato sul Quest 3 il 7 settembre tramite ADB (`Success`). Avvio richiesto: il sistema mostra la richiesta di attivare i controller prima di aprire l'app. Nuovi comandi e caricamento di Courmayeur ancora da confermare nel visore.
- Aggiornamento successivo: turbo x5; pannelli laterali e crediti al bordo inferiore; volo libero selezionabile col clic dello stick sinistro, senza calibrazione o tracking posizionale delle mani; eliminato il limite di 4,5 km nella modalità Google.
- Verifica offline `FlightChecks.Run`: superati 250 passi da 500 m (125 km), controllo della posizione geografica prima/dopo ciascun riposizionamento, conservazione della posa locale degli occhi, ritorno a Courmayeur e Roma. Nessuna richiesta cartografica eseguita da questo test.
- Il caricamento effettivo alle velocità turbo, il layout laterale nel visore e il volo libero restano da provare. ADB non rileva dispositivi durante la preparazione di questo aggiornamento.
- Dopo il ricollegamento, APK con turbo x5, HUD laterale, volo libero e origine mobile installato con successo sul Quest 3. Avvio diretto dell'attività riuscito e processo presente. Verifica percettiva e prestazioni ancora da confermare dall'utente.
- Correzione tracking: il volo libero ora parte come modalità predefinita e la posa del visore viene verificata tramite gli stati XR del centro occhi/visore, con ripiego sul dispositivo Head. Le informazioni sono state spostate in basso, con sfondo scuro e testo più grande.
- Aggiunti in volo libero grilletto destro per salire e grilletto sinistro per scendere. Velocità libera raddoppiata a 90 m/s, turbo x5 fino a 450 m/s. Streaming: cache 512 MiB, 10 caricamenti paralleli, SSE 20 e limite discendenti 40. Etichette locali dei comuni create per Valle d'Aosta e area di Roma.
- APK installato con successo sul Quest 3; è necessaria la riattivazione dei controller dal dialogo di sistema prima dell'avvio e una verifica percettiva di comandi, leggibilità e prestazioni.

Questi controlli verificano compilazione e confezionamento, non sostituiscono un test sul Quest 3.
