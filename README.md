# Earth Wings — prototipo Quest 3 standalone

Progetto Unity 6000.6.0f1 per Android ARM64/OpenXR. Non richiede un PC durante il gioco. Le mappe reali richiedono Internet e un account abilitato al servizio cartografico.

## Avvio

1. Su una nuova copia del repository eseguire `./Fetch-Cesium.ps1` (il pacchetto ufficiale Cesium 1.25.1 è già scaricato in questa cartella). Aprire questa cartella da Unity Hub e attendere l'importazione dei pacchetti.
2. Eseguire **Earth Wings > Prepare Quest project**.
3. Eseguire **Earth Wings > Build Quest APK** con Android SDK, NDK e OpenJDK configurati in Unity.
4. Installare `Builds/EarthWings.apk` sul visore in modalità sviluppatore, tramite Meta Quest Developer Hub o `adb install -r Builds/EarthWings.apk`.

La scena viene generata dal comando Prepare: non modificarla per salvare personalizzazioni, perché il comando la ricrea.

## Comandi

- A sul controller destro: partire / mettere in pausa.
- Durante il volo, tenere A: turbo x5.
- Grip click destro: aumenta la velocità di crociera; grip click sinistro: la riduce.
- Stick destro sinistra/destra: vira.
- Grilletto destro: salita; grilletto sinistro: discesa.
- X sul controller sinistro, tenuto premuto: turbo x5.
- Y sul controller sinistro: torna al punto di lancio e mette il volo in pausa.
- B sul controller destro: alterna Courmayeur (Valle d'Aosta) e Roma. Il cambio riporta in quota e in pausa; attendere il terreno e premere A.
- L'app parte in volo libero, in pausa. Il clic sullo stick sinistro alterna volo libero e tuta alare. La tuta alare richiede una calibrazione: braccia aperte e entrambi i grilletti premuti per 1,5 secondi.
- La perdita del tracking del visore o del focus mette in pausa. Nel volo libero la perdita del tracking posizionale delle mani non interrompe il volo.

La visuale segue liberamente il visore e mantiene l'orizzonte verticale. Il modello di volo è arcade, non una simulazione aerodinamica. La calibrazione utilizza la distanza fra i controller: braccia completamente fuori dal campo di tracking possono causare una pausa, da verificare sul Quest.

## Mappe Google

Senza configurazione si usa un terreno sintetico con torri, riconoscibile come AREA DI ADDESTRAMENTO. Non è Google Earth.

Seguire https://cesium.com/learn/unity/unity-photorealistic-3d-tiles/ per abilitare Google Photorealistic 3D Tiles nel proprio account Cesium ion. Creare `Assets/Resources/MapCredentials.json` localmente (il file è escluso da Git):

```json
{
  "ionToken": "TOKEN_CON_ACCESSO_AL_SOLO_ASSET",
  "assetId": 0,
  "latitude": 45.7874,
  "longitude": 6.9731,
  "originHeight": 0,
  "launchHeight": 3200
}
```

Sostituire assetId con l'identificativo effettivo dell'asset nel proprio account. Le coordinate iniziali sono nell'area del Monte Bianco: disponibilità e dettaglio della copertura vanno verificati. launchHeight è relativo all'origine geografica, non altezza dal terreno. Un token incluso nell'APK è estraibile: usare credenziali con privilegi minimi e limiti di servizio adeguati.

L'integrazione attiva mesh fisiche e crediti Cesium. Le attribuzioni devono essere leggibili in VR prima di distribuire una versione con mappe: https://developers.google.com/maps/documentation/tile/policies . Nessuna mappa viene scaricata offline da questo progetto.

## Immagini

Le schermate VR vengono acquisite da un Quest collegato con `adb exec-out screencap -p`. Non sono incluse nel repository quando il visore non è connesso: nessuna immagine di esempio viene generata artificialmente.

## Limiti del prototipo

Obiettivo 72 fps, ancora da misurare sul visore. Rendering MultiPass confermato dall'utente per correggere lo sdoppiamento della mappa. Streaming di sei tile simultanee, cache 256 MiB e precaricamento dei livelli superiori e tasselli vicini. Nessun confine radiale nella modalità Google: l'origine geografica segue il pilota ogni 1000 m, preservando la posizione sul globo. L'area sintetica resta limitata a 4,5 km. Copertura e dettaglio dipendono dai dati Google; a velocità elevate il caricamento può restare indietro rispetto al volo. Le collisioni dipendono dalle mesh già caricate: non garantiscono protezione sul terreno ancora in streaming. Non riprendere il volo sulle mappe finché il terreno non è visibile.

Informazioni e comandi sono raccolti in basso, con caratteri più grandi e sfondo scuro; le attribuzioni cartografiche sono subito sotto. Il centro della visuale resta libero. Il messaggio di tracking distingue fra visore e mani e indica quando il segnale ritorna; premere A per riprendere.

Il comando Build esegue un controllo offline di uno spostamento da 125 km, riposizionamento dell'origine, invariabilità della posa locale degli occhi e ritorno ai punti di partenza. Non misura streaming o prestazioni sul Quest.

## Fonti tecniche

- https://cesium.com/learn/cesium-unity/ref-doc/supported-platforms.html
- https://developers.meta.com/horizon/documentation/unity/unity-xr-plugin/
