[← Go Back](https://github.com/ewang2002/RealmEyeSharper/tree/master/RealmAspNet/docs/docs-guide.md)

# RealmEye API → Player API → Basics
This endpoint gives you the data that is displayed on the player's homepage.

## Endpoint

```
// Gets an unformatted JSON.
GET api/realmeye/player/basics?name={name}

// Gets a formatted JSON.
GET api/realmeye/player/basics?name={name}&prettify
```

### Arguments

| Argument Name | Type | Required? | Description |
| ---- | ---- | --------- | ----------- |
| `name` | `string` | Yes | The name of the player to request. This is case-insensitive. | 
| `prettify` | | No | If included, returns a formatted JSON. |

## TypeScript Definition 
The corresponding TypeScript definition is:
```ts 
interface IPlayerData extends IRealmEyePlayerResponse {
    characterCount: number;
    skins: number;
    fame: number;
    exp: number;
    rank: number;
    accountFame: number;
    guild: string;
    guildRank: string;
    firstSeen?: string;
    created?: string;
    lastSeen: string;
    description: string[];
    characters: {
        pet: {
            name: string;
            id: number;
        };
        characterSkin: {
            clothingDyeId: number;
            clothingDyeName: string;
            accessoryDyeId: number;
            accessoryDyeName: string;
            skinId: number;
        };
        characterType: string;
        level: number;
        classQuestsCompleted: number;
        fame: number;
        experience: number;
        place: number;
        equipmentData: {
            name: string;
            tier: string;
            id: number;
        }[];
        hasBackpack: boolean;
        stats: {
            Health: number;
            Magic: number;
            Attack: number;
            Defense: number;
            Speed: number;
            Vitality: number;
            Wisdom: number;
            Dexterity: number;
        };
        statsMaxed: number;
    }[];
}
```

## Examples
Here are some examples. 

### Case 1 
```
GET api/realmeye/player/basics?name=Darkmattr
```
Response:
```json
{"characterCount":0,"skins":34,"fame":0,"exp":0,"rank":47,"accountFame":37770,"created":"~8 years and 308 days ago","lastSeen":"2021-06-29 19:48:10 at USWest3 as Paladin","description":["Developer of the Ooga-Booga Discord Bot","used for ROTMG raid/guild discord servers.","Contact me if you would like the bot added to your server."],"characters":[],"profileIsPrivate":false,"sectionIsPrivate":false,"name":"Darkmattr","resultCode":200}
```

### Case 2 
``` 
GET api/realmeye/player/basics?name=kbgwhbgs
```
Response: 
```json
{"profileIsPrivate":true,"sectionIsPrivate":true,"name":"kbgwhbgs","resultCode":400}
```

### Case 3
```
GET api/realmeye/player/basics?name=Deatttthhh&prettify
```
Response: 
```json
{
  "characterCount": 6,
  "skins": 63,
  "fame": 2283,
  "exp": 4332170,
  "rank": 61,
  "accountFame": 34853,
  "guild": "CONTENT",
  "guildRank": "Member",
  "firstSeen": "~3 years and 146 days ago",
  "lastSeen": "hidden",
  "description": [
    "Death is everywhere",
    "Back to orange star",
    "Nothing"
  ],
  "characters": [
    {
      "pet": {
        "name": "Krampus",
        "id": 24615
      },
      "characterSkin": {
        "clothingDyeId": 0,
        "clothingDyeName": "",
        "accessoryDyeId": 0,
        "accessoryDyeName": "",
        "skinId": 30733
      },
      "characterType": "Bard",
      "level": 20,
      "classQuestsCompleted": -1,
      "fame": 20,
      "experience": 648,
      "place": 6580,
      "equipmentData": [
        {
          "name": "Bow of Covert Havens",
          "tier": "T12",
          "id": 2818
        },
        {
          "name": "Wavecrest Concertina",
          "tier": "UT",
          "id": 19948
        },
        {
          "name": "Robe of the Star Mother",
          "tier": "T14",
          "id": 2511
        },
        {
          "name": "Ring of Decades",
          "tier": "UT",
          "id": 2990
        }
      ],
      "hasBackpack": false,
      "stats": {
        "Health": 748,
        "Magic": 359,
        "Attack": -1,
        "Defense": -1,
        "Speed": -1,
        "Vitality": -1,
        "Wisdom": -1,
        "Dexterity": -1
      },
      "statsMaxed": -1
    },
    // ... omitted
    {
      "pet": {
        "name": "Krampus",
        "id": 24615
      },
      "characterSkin": {
        "clothingDyeId": 0,
        "clothingDyeName": "",
        "accessoryDyeId": 4667,
        "accessoryDyeName": "Large Teal Crystal Cloth",
        "skinId": 29787
      },
      "characterType": "Mystic",
      "level": 20,
      "classQuestsCompleted": -1,
      "fame": 22,
      "experience": 4342,
      "place": 5722,
      "equipmentData": [
        {
          "name": "The Phylactery",
          "tier": "UT",
          "id": 9059
        },
        {
          "name": "Soul of the Bearer",
          "tier": "UT",
          "id": 9058
        },
        {
          "name": "Soulless Robe",
          "tier": "UT",
          "id": 9056
        },
        {
          "name": "Ring of the Covetous Heart",
          "tier": "UT",
          "id": 9057
        }
      ],
      "hasBackpack": false,
      "stats": {
        "Health": 780,
        "Magic": 495,
        "Attack": -1,
        "Defense": -1,
        "Speed": -1,
        "Vitality": -1,
        "Wisdom": -1,
        "Dexterity": -1
      },
      "statsMaxed": -1
    }
  ],
  "profileIsPrivate": false,
  "sectionIsPrivate": false,
  "name": "Deatttthhh",
  "resultCode": 200
}
```