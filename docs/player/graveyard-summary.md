[← Go Back](https://github.com/ewang2002/RealmEyeSharper/tree/master/RealmAspNet/docs/docs-guide.md)

# RealmEye API → Player API → Graveyard Summary
This endpoint gives you the data that is displayed on the player's graveyard summary page.

## Endpoint

```
// Gets an unformatted JSON.
GET api/realmeye/player/graveyardsummary?name={name}

// Gets a formatted JSON.
GET api/realmeye/player/graveyardsummary?name={name}&prettify
```

### Arguments

| Argument Name | Type | Required? | Description |
| ---- | ---- | --------- | ----------- |
| `name` | `string` | Yes | The name of the player to request. This is case-insensitive. |
| `prettify` | | No | If included, returns a formatted JSON. |

## TypeScript Definition 
The corresponding TypeScript definition is:
```ts 
interface IGraveyardSummary extends IRealmEyePlayerResponse {
    properties: {
        achievement: string;
        total: number;
        max: number;
        average: number;
        min: number;
    }[];

    technicalProperties: {
        achievement: string;
        total: string;
        max: string;
        average: string;
        min: string;
    }[];

    statsCharacters: {
        characterType: string;
        stats: number[];
        total: number;
    }[];
}
```

## Examples
Here are some examples. 

### Case 1 
```
GET api/realmeye/player/graveyardsummary?name=darkmattr
```
Response:
```json
{"properties":[{"achievement":"Base fame","total":144558,"max":22755,"average":478.67,"min":0},{"achievement":"Total fame","total":252785,"max":40954,"average":837.04,"min":0},{"achievement":"Oryx kills","total":24,"max":5,"average":0.08,"min":0},{"achievement":"God kills","total":24070,"max":5612,"average":79.7,"min":0},{"achievement":"Monster kills","total":341973,"max":46243,"average":1132.36,"min":0},{"achievement":"Quests completed","total":12583,"max":1576,"average":41.67,"min":0},{"achievement":"Tiles uncovered","total":140102852,"max":14724728,"average":463916.73,"min":2722},{"achievement":"Lost Halls completed","total":23,"max":6,"average":0.08,"min":0},{"achievement":"Voids completed","total":110,"max":24,"average":0.36,"min":0},{"achievement":"Cultist Hideouts completed","total":84,"max":20,"average":0.28,"min":0},{"achievement":"Nests completed2","total":50,"max":15,"average":0.17,"min":0},{"achievement":"Shatters completed1","total":68,"max":27,"average":0.23,"min":0},{"achievement":"Tombs completed","total":94,"max":25,"average":0.31,"min":0},{"achievement":"Ocean Trenches completed","total":145,"max":33,"average":0.48,"min":0},{"achievement":"Parasite chambers completed4","total":98,"max":31,"average":0.32,"min":0},{"achievement":"Lairs of Shaitan completed4","total":14,"max":6,"average":0.05,"min":0},{"achievement":"Puppet Master's Encores completed4","total":29,"max":8,"average":0.1,"min":0},{"achievement":"Cnidarian Reefs completed","total":22,"max":5,"average":0.07,"min":0},{"achievement":"Secluded Thickets completed","total":41,"max":22,"average":0.14,"min":0},{"achievement":"Cursed Libraries completed","total":93,"max":46,"average":0.31,"min":0},{"achievement":"Fungal Caverns completed","total":17,"max":13,"average":0.06,"min":0},{"achievement":"Crystal Caverns completed","total":42,"max":24,"average":0.14,"min":0},{"achievement":"Lairs of Draconis (hard mode) completed2","total":0,"max":0,"average":0,"min":0},{"achievement":"Lairs of Draconis (easy mode) completed1","total":19,"max":6,"average":0.06,"min":0},{"achievement":"Mountain Temples completed2","total":52,"max":14,"average":0.17,"min":0},{"achievement":"Crawling Depths completed1","total":76,"max":17,"average":0.25,"min":0},{"achievement":"Woodland Labyrinths completed1","total":59,"max":21,"average":0.2,"min":0},{"achievement":"Deadwater Docks completed1","total":202,"max":32,"average":0.67,"min":0},{"achievement":"Ice Caves completed1","total":67,"max":38,"average":0.22,"min":0},{"achievement":"Bella Donnas completed3","total":4,"max":2,"average":0.01,"min":0},{"achievement":"Davy Jones's Lockers completed1","total":146,"max":36,"average":0.48,"min":0},{"achievement":"Battle for the Nexuses completed1","total":7,"max":3,"average":0.02,"min":0},{"achievement":"Candyland Hunting Grounds completed","total":8,"max":3,"average":0.03,"min":0},{"achievement":"Puppet Master's Theatres completed1","total":35,"max":14,"average":0.12,"min":0},{"achievement":"Toxic Sewers completed1","total":55,"max":26,"average":0.18,"min":0},{"achievement":"Haunted Cemeteries completed1","total":11,"max":3,"average":0.04,"min":0},{"achievement":"Mad Labs completed1","total":42,"max":7,"average":0.14,"min":0},{"achievement":"Abysses of Demons completed","total":317,"max":96,"average":1.05,"min":0},{"achievement":"Manors of the Immortals completed","total":146,"max":29,"average":0.48,"min":0},{"achievement":"Ancient Ruins completed","total":11,"max":6,"average":0.04,"min":0},{"achievement":"Undead Lairs completed","total":291,"max":61,"average":0.96,"min":0},{"achievement":"Sprite Worlds completed","total":387,"max":85,"average":1.28,"min":0},{"achievement":"Snake Pits completed","total":198,"max":53,"average":0.66,"min":0},{"achievement":"Caves of a Thousand Treasures completed1","total":17,"max":4,"average":0.06,"min":0},{"achievement":"Magic Woods completed","total":40,"max":10,"average":0.13,"min":0},{"achievement":"Hives completed1","total":4,"max":2,"average":0.01,"min":0},{"achievement":"Spider Dens completed","total":41,"max":3,"average":0.14,"min":0},{"achievement":"Forbidden Jungles completed","total":70,"max":5,"average":0.23,"min":0},{"achievement":"Forest Mazes completed1","total":5,"max":1,"average":0.02,"min":0},{"achievement":"Pirate Caves completed","total":154,"max":97,"average":0.51,"min":0}],"technicalProperties":[{"achievement":"God kill assists","total":"117532","max":"26647","average":"389.2","min":"0"},{"achievement":"Monster kill assists","total":"612539","max":"67390","average":"2028.3","min":"0"},{"achievement":"Shots","total":"11348903","max":"1568459","average":"37579.1","min":"0"},{"achievement":"Hits","total":"4051110","max":"789483","average":"13414.3","min":"0"},{"achievement":"Ability uses","total":"221093","max":"34698","average":"732.1","min":"0"},{"achievement":"Teleports","total":"29395","max":"10151","average":"97.3","min":"0"},{"achievement":"Potions drunk","total":"5478","max":"333","average":"18.1","min":"0"},{"achievement":"Cube kills","total":"1865","max":"647","average":"6.2","min":"0"},{"achievement":"Level up assists","total":"1441","max":"193","average":"4.8","min":"0"},{"achievement":"Active for","total":"32d 6h 31m","max":"4d 3h 19m","average":"2h 34m","min":"0m"},{"achievement":"Fame bonuses","total":"108227","max":"18199","average":"358.4","min":"0"},{"achievement":"Accuracy","total":"35.7%","max":"113.92%","average":"25.7%","min":"0%"}],"statsCharacters":[{"characterType":"Rogue","stats":[20,0,0,0,0,0,0,0,0],"total":20},{"characterType":"Archer","stats":[29,1,0,0,0,0,1,0,0],"total":31},{"characterType":"Wizard","stats":[71,0,0,0,0,0,0,0,4],"total":75},{"characterType":"Priest","stats":[6,0,0,0,0,0,0,0,4],"total":10},{"characterType":"Warrior","stats":[24,0,0,0,0,2,0,1,18],"total":45},{"characterType":"Knight","stats":[41,1,3,0,0,1,2,2,4],"total":54},{"characterType":"Paladin","stats":[4,0,0,1,0,1,0,0,8],"total":14},{"characterType":"Assassin","stats":[18,0,0,1,0,0,0,0,1],"total":20},{"characterType":"Necromancer","stats":[7,0,0,0,0,0,0,0,0],"total":7},{"characterType":"Huntress","stats":[2,0,0,0,0,0,0,0,1],"total":3},{"characterType":"Mystic","stats":[2,0,0,0,0,0,0,0,1],"total":3},{"characterType":"Trickster","stats":[1,0,0,0,0,0,0,0,2],"total":3},{"characterType":"Sorcerer","stats":[6,0,0,1,0,0,0,0,0],"total":7},{"characterType":"Ninja","stats":[6,0,0,0,0,0,0,1,0],"total":7},{"characterType":"Samurai","stats":[1,0,1,0,0,0,0,0,0],"total":2},{"characterType":"Bard","stats":[1,0,0,0,0,0,0,0,0],"total":1},{"characterType":"Summoner","stats":[0,0,0,0,0,0,0,0,0],"total":0}],"profileIsPrivate":false,"sectionIsPrivate":false,"name":"darkmattr","resultCode":200}
```

### Case 2 
``` 
GET api/realmeye/player/graveyardsummary?name=deatttthhh&prettify
```
Response: 
```json
{
  "properties": [
    {
      "achievement": "Base fame",
      "total": 436866,
      "max": 37375,
      "average": 1835.57,
      "min": 0
    },
    {
      "achievement": "Total fame",
      "total": 835839,
      "max": 84668,
      "average": 3511.93,
      "min": 0
    },
    {
      "achievement": "Oryx kills",
      "total": 753,
      "max": 151,
      "average": 3.16,
      "min": 0
    },
    {
      "achievement": "God kills",
      "total": 52417,
      "max": 2439,
      "average": 220.24,
      "min": 0
    },
    {
      "achievement": "Monster kills",
      "total": 528683,
      "max": 29521,
      "average": 2221.36,
      "min": 0
    },
    {
      "achievement": "Quests completed",
      "total": 31424,
      "max": 944,
      "average": 132.03,
      "min": 0
    },
    {
      "achievement": "Tiles uncovered",
      "total": 285520718,
      "max": 13542777,
      "average": 1199666.88,
      "min": 3077
    },
    {
      "achievement": "Lost Halls completed",
      "total": 38,
      "max": 12,
      "average": 0.16,
      "min": 0
    },
    {
      "achievement": "Voids completed",
      "total": 273,
      "max": 25,
      "average": 1.15,
      "min": 0
    },
    {
      "achievement": "Cultist Hideouts completed",
      "total": 533,
      "max": 53,
      "average": 2.24,
      "min": 0
    },
    {
      "achievement": "Nests completed2",
      "total": 214,
      "max": 62,
      "average": 0.9,
      "min": 0
    },
    {
      "achievement": "Shatters completed1",
      "total": 171,
      "max": 27,
      "average": 0.72,
      "min": 0
    },
    {
      "achievement": "Tombs completed",
      "total": 206,
      "max": 32,
      "average": 0.87,
      "min": 0
    },
    {
      "achievement": "Ocean Trenches completed",
      "total": 291,
      "max": 63,
      "average": 1.22,
      "min": 0
    },
    {
      "achievement": "Parasite chambers completed4",
      "total": 305,
      "max": 173,
      "average": 1.28,
      "min": 0
    },
    {
      "achievement": "Lairs of Shaitan completed4",
      "total": 278,
      "max": 63,
      "average": 1.17,
      "min": 0
    },
    {
      "achievement": "Puppet Master's Encores completed4",
      "total": 212,
      "max": 51,
      "average": 0.89,
      "min": 0
    },
    {
      "achievement": "Cnidarian Reefs completed",
      "total": 140,
      "max": 38,
      "average": 0.59,
      "min": 0
    },
    {
      "achievement": "Secluded Thickets completed",
      "total": 197,
      "max": 43,
      "average": 0.83,
      "min": 0
    },
    {
      "achievement": "Cursed Libraries completed",
      "total": 82,
      "max": 17,
      "average": 0.34,
      "min": 0
    },
    {
      "achievement": "Fungal Caverns completed",
      "total": 216,
      "max": 39,
      "average": 0.91,
      "min": 0
    },
    {
      "achievement": "Crystal Caverns completed",
      "total": 173,
      "max": 30,
      "average": 0.73,
      "min": 0
    },
    {
      "achievement": "Lairs of Draconis (hard mode) completed2",
      "total": 54,
      "max": 26,
      "average": 0.23,
      "min": 0
    },
    {
      "achievement": "Lairs of Draconis (easy mode) completed1",
      "total": 70,
      "max": 15,
      "average": 0.29,
      "min": 0
    },
    {
      "achievement": "Mountain Temples completed2",
      "total": 198,
      "max": 39,
      "average": 0.83,
      "min": 0
    },
    {
      "achievement": "Crawling Depths completed1",
      "total": 372,
      "max": 115,
      "average": 1.56,
      "min": 0
    },
    {
      "achievement": "Woodland Labyrinths completed1",
      "total": 387,
      "max": 73,
      "average": 1.63,
      "min": 0
    },
    {
      "achievement": "Deadwater Docks completed1",
      "total": 544,
      "max": 32,
      "average": 2.29,
      "min": 0
    },
    {
      "achievement": "Ice Caves completed1",
      "total": 215,
      "max": 50,
      "average": 0.9,
      "min": 0
    },
    {
      "achievement": "Bella Donnas completed3",
      "total": 128,
      "max": 95,
      "average": 0.54,
      "min": 0
    },
    {
      "achievement": "Davy Jones's Lockers completed1",
      "total": 367,
      "max": 52,
      "average": 1.54,
      "min": 0
    },
    {
      "achievement": "Battle for the Nexuses completed1",
      "total": 12,
      "max": 2,
      "average": 0.05,
      "min": 0
    },
    {
      "achievement": "Candyland Hunting Grounds completed",
      "total": 1,
      "max": 1,
      "average": 0,
      "min": 0
    },
    {
      "achievement": "Puppet Master's Theatres completed1",
      "total": 357,
      "max": 37,
      "average": 1.5,
      "min": 0
    },
    {
      "achievement": "Toxic Sewers completed1",
      "total": 333,
      "max": 56,
      "average": 1.4,
      "min": 0
    },
    {
      "achievement": "Haunted Cemeteries completed1",
      "total": 156,
      "max": 21,
      "average": 0.66,
      "min": 0
    },
    {
      "achievement": "Mad Labs completed1",
      "total": 228,
      "max": 27,
      "average": 0.96,
      "min": 0
    },
    {
      "achievement": "Abysses of Demons completed",
      "total": 611,
      "max": 267,
      "average": 2.57,
      "min": 0
    },
    {
      "achievement": "Manors of the Immortals completed",
      "total": 168,
      "max": 29,
      "average": 0.71,
      "min": 0
    },
    {
      "achievement": "Ancient Ruins completed",
      "total": 14,
      "max": 7,
      "average": 0.06,
      "min": 0
    },
    {
      "achievement": "Undead Lairs completed",
      "total": 560,
      "max": 55,
      "average": 2.35,
      "min": 0
    },
    {
      "achievement": "Sprite Worlds completed",
      "total": 1211,
      "max": 50,
      "average": 5.09,
      "min": 0
    },
    {
      "achievement": "Snake Pits completed",
      "total": 606,
      "max": 28,
      "average": 2.55,
      "min": 0
    },
    {
      "achievement": "Caves of a Thousand Treasures completed1",
      "total": 37,
      "max": 5,
      "average": 0.16,
      "min": 0
    },
    {
      "achievement": "Magic Woods completed",
      "total": 152,
      "max": 13,
      "average": 0.64,
      "min": 0
    },
    {
      "achievement": "Hives completed1",
      "total": 9,
      "max": 1,
      "average": 0.04,
      "min": 0
    },
    {
      "achievement": "Spider Dens completed",
      "total": 221,
      "max": 55,
      "average": 0.93,
      "min": 0
    },
    {
      "achievement": "Forbidden Jungles completed",
      "total": 16,
      "max": 2,
      "average": 0.07,
      "min": 0
    },
    {
      "achievement": "Forest Mazes completed1",
      "total": 0,
      "max": 0,
      "average": 0,
      "min": 0
    },
    {
      "achievement": "Pirate Caves completed",
      "total": 18,
      "max": 2,
      "average": 0.08,
      "min": 0
    }
  ],
  "technicalProperties": [
    {
      "achievement": "God kill assists",
      "total": "198206",
      "max": "26627",
      "average": "832.8",
      "min": "0"
    },
    {
      "achievement": "Monster kill assists",
      "total": "1462863",
      "max": "76943",
      "average": "6146.5",
      "min": "0"
    },
    {
      "achievement": "Shots",
      "total": "45426202",
      "max": "3210848",
      "average": "190866.4",
      "min": "8"
    },
    {
      "achievement": "Hits",
      "total": "8515291",
      "max": "484752",
      "average": "35778.5",
      "min": "0"
    },
    {
      "achievement": "Ability uses",
      "total": "560425",
      "max": "29322",
      "average": "2354.7",
      "min": "0"
    },
    {
      "achievement": "Teleports",
      "total": "40465",
      "max": "1839",
      "average": "170",
      "min": "0"
    },
    {
      "achievement": "Potions drunk",
      "total": "12630",
      "max": "610",
      "average": "53.1",
      "min": "0"
    },
    {
      "achievement": "Cube kills",
      "total": "2209",
      "max": "267",
      "average": "9.3",
      "min": "0"
    },
    {
      "achievement": "Level up assists",
      "total": "5913",
      "max": "546",
      "average": "24.8",
      "min": "0"
    },
    {
      "achievement": "Active for",
      "total": "84d 21h 46m",
      "max": "3d 9h 13m",
      "average": "8h 34m",
      "min": "0m"
    },
    {
      "achievement": "Fame bonuses",
      "total": "398973",
      "max": "47293",
      "average": "1676.4",
      "min": "0"
    },
    {
      "achievement": "Accuracy",
      "total": "18.75%",
      "max": "60%",
      "average": "19.88%",
      "min": "0%"
    }
  ],
  "statsCharacters": [
    {
      "characterType": "Rogue",
      "stats": [
        1,
        0,
        0,
        1,
        0,
        0,
        1,
        0,
        2
      ],
      "total": 5
    },
    {
      "characterType": "Archer",
      "stats": [
        6,
        0,
        0,
        0,
        0,
        1,
        0,
        1,
        3
      ],
      "total": 11
    },
    {
      "characterType": "Wizard",
      "stats": [
        17,
        1,
        1,
        0,
        1,
        1,
        0,
        1,
        3
      ],
      "total": 25
    },
    {
      "characterType": "Priest",
      "stats": [
        6,
        0,
        0,
        0,
        1,
        1,
        0,
        0,
        8
      ],
      "total": 16
    },
    {
      "characterType": "Warrior",
      "stats": [
        3,
        2,
        2,
        1,
        2,
        0,
        1,
        3,
        13
      ],
      "total": 27
    },
    {
      "characterType": "Knight",
      "stats": [
        11,
        3,
        4,
        1,
        2,
        1,
        2,
        3,
        5
      ],
      "total": 32
    },
    {
      "characterType": "Paladin",
      "stats": [
        11,
        4,
        3,
        1,
        1,
        2,
        1,
        0,
        14
      ],
      "total": 37
    },
    {
      "characterType": "Assassin",
      "stats": [
        13,
        0,
        0,
        0,
        0,
        0,
        0,
        1,
        0
      ],
      "total": 14
    },
    {
      "characterType": "Necromancer",
      "stats": [
        11,
        0,
        1,
        0,
        0,
        0,
        0,
        0,
        1
      ],
      "total": 13
    },
    {
      "characterType": "Huntress",
      "stats": [
        6,
        0,
        0,
        0,
        0,
        1,
        1,
        0,
        1
      ],
      "total": 9
    },
    {
      "characterType": "Mystic",
      "stats": [
        2,
        0,
        0,
        0,
        0,
        1,
        1,
        0,
        2
      ],
      "total": 6
    },
    {
      "characterType": "Trickster",
      "stats": [
        3,
        0,
        0,
        0,
        0,
        1,
        0,
        1,
        0
      ],
      "total": 5
    },
    {
      "characterType": "Sorcerer",
      "stats": [
        1,
        0,
        0,
        0,
        0,
        1,
        0,
        0,
        1
      ],
      "total": 3
    },
    {
      "characterType": "Ninja",
      "stats": [
        10,
        0,
        0,
        1,
        1,
        0,
        0,
        0,
        4
      ],
      "total": 16
    },
    {
      "characterType": "Samurai",
      "stats": [
        5,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        2
      ],
      "total": 7
    },
    {
      "characterType": "Bard",
      "stats": [
        4,
        0,
        0,
        1,
        0,
        0,
        0,
        0,
        1
      ],
      "total": 6
    },
    {
      "characterType": "Summoner",
      "stats": [
        3,
        1,
        0,
        0,
        0,
        0,
        1,
        0,
        1
      ],
      "total": 6
    }
  ],
  "profileIsPrivate": false,
  "sectionIsPrivate": false,
  "name": "deatttthhh",
  "resultCode": 200
}
```