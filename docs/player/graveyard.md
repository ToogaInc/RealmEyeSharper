[← Go Back](https://github.com/ewang2002/RealmEyeSharper/blob/master/docs/docs-guide.md)

# RealmEye API → Player API → Graveyard
This endpoint gives you the data that is displayed on the player's graveyard page.

## Endpoint

```
// Gets an unformatted JSON.
GET api/realmeye/player/gravayard?name={name}

// Gets a formatted JSON.
GET api/realmeye/player/petyard?name={name}&prettify
```

### Arguments

| Argument Name | Type | Required? | Description |
| ---- | ---- | --------- | ----------- |
| `name` | `string` | Yes | The name of the player to request. This is case-insensitive. |
| `limit` | `number` | No | The number of entries to retrieve. By default, this will return 100 entries. |
| `prettify` | | No | If included, returns a formatted JSON. |

Note: You can only request up to 100 of the most recent deaths. 

## TypeScript Definition 
The corresponding TypeScript definition is:
```ts 
interface IGraveyard extends IRealmEyePlayerResponse {
    graveyardCount: number;
    graveyard: {
        diedOn: string;
        character: string;
        level: number;
        baseFame: number;
        totalFame: number;
        experience: number;
        equipment: string[];
        maxedStats: number;
        killedBy: string;
        hadBackpack: boolean;
    }[];
}
```
Note: `graveyardCount` returns the total entries (i.e. total number of deaths recorded by RealmEye), regardless of what was actually requested.

## Examples
Here are some examples. 

### Case 1 
```
GET api/realmeye/player/graveyard?name=darkmattr&limit=1
```
Response:
```json
{"graveyardCount":302,"graveyard":[{"diedOn":"2020-11-20T19:40:22Z","characterSkin":{"clothingDyeId":4980,"clothingDyeName":"Small Dammah Cloth","accessoryDyeId":0,"accessoryDyeName":"","skinId":9453},"character":"Warrior","level":20,"baseFame":3741,"totalFame":5050,"experience":7090527,"equipment":[{"name":"Sword of Acclaim","tier":"T12","id":2827},{"name":"Helm of the Great General","tier":"T6","id":2857},{"name":"Annihilation Armor","tier":"T15","id":2501},{"name":"Ring of Unbound Health","tier":"T6","id":2985}],"maxedStats":8,"killedBy":"O3B Knight","hadBackpack":true},{"diedOn":"2020-11-12T04:25:03Z","characterSkin":{"clothingDyeId":0,"clothingDyeName":"","accessoryDyeId":0,"accessoryDyeName":"","skinId":10157},"character":"Warrior","level":5,"baseFame":0,"totalFame":0,"experience":960,"equipment":[{"name":"Short Sword","tier":"T0","id":2560},{"name":"Combat Helm","tier":"T0","id":2662},{"name":"Empty Slot","tier":"","id":-1},{"name":"Empty Slot","tier":"","id":-1}],"maxedStats":0,"killedBy":"Demon of the Abyss","hadBackpack":false}],"profileIsPrivate":false,"sectionIsPrivate":false,"name":"darkmattr","resultCode":200}
```

### Case 2 
``` 
GET api/realmeye/player/graveyard?name=darkmattr&limit=5&prettify
```
Response: 
```json
{
  "graveyardCount": 302,
  "graveyard": [
    {
      "diedOn": "2020-11-20T19:40:22Z",
      "characterSkin": {
        "clothingDyeId": 4980,
        "clothingDyeName": "Small Dammah Cloth",
        "accessoryDyeId": 0,
        "accessoryDyeName": "",
        "skinId": 9453
      },
      "character": "Warrior",
      "level": 20,
      "baseFame": 3741,
      "totalFame": 5050,
      "experience": 7090527,
      "equipment": [
        {
          "name": "Sword of Acclaim",
          "tier": "T12",
          "id": 2827
        },
        {
          "name": "Helm of the Great General",
          "tier": "T6",
          "id": 2857
        },
        {
          "name": "Annihilation Armor",
          "tier": "T15",
          "id": 2501
        },
        {
          "name": "Ring of Unbound Health",
          "tier": "T6",
          "id": 2985
        }
      ],
      "maxedStats": 8,
      "killedBy": "O3B Knight",
      "hadBackpack": true
    },
    // omitted 3 entries
    {
      "diedOn": "2020-02-07T08:35:56Z",
      "characterSkin": {
        "clothingDyeId": 4424,
        "clothingDyeName": "Light Sky Blue Accessory Dye",
        "accessoryDyeId": 0,
        "accessoryDyeName": "",
        "skinId": 0
      },
      "character": "Samurai",
      "level": 20,
      "baseFame": 872,
      "totalFame": 2254,
      "experience": 1352689,
      "equipment": [
        {
          "name": "Masamune",
          "tier": "T12",
          "id": 3152
        },
        {
          "name": "Royal Wakizashi",
          "tier": "T6",
          "id": 6832
        },
        {
          "name": "Acropolis Armor",
          "tier": "T13",
          "id": 2812
        },
        {
          "name": "Ring of Unbound Magic",
          "tier": "T6",
          "id": 2986
        }
      ],
      "maxedStats": 2,
      "killedBy": "Son of Arachna",
      "hadBackpack": true
    }
  ],
  "profileIsPrivate": false,
  "sectionIsPrivate": false,
  "name": "darkmattr",
  "resultCode": 200
}
```