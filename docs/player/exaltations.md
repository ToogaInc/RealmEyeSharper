[← Go Back](https://github.com/ewang2002/RealmEyeSharper/blob/master/docs/docs-guide.md)

# RealmEye API → Player API → Exaltations
This endpoint gives you the data that is displayed on the player's exaltations page.

## Endpoint

```
// Gets an unformatted JSON.
GET api/realmeye/player/exaltations?name={name}

// Gets a formatted JSON.
GET api/realmeye/player/exaltations?name={name}&prettify
```

### Arguments

| Argument Name | Type | Required? | Description |
| ---- | ---- | --------- | ----------- |
| `name` | `string` | Yes | The name of the player to request. This is case-insensitive. |
| `prettify` | | No | If included, returns a formatted JSON. |

## TypeScript Definition 
The corresponding TypeScript definition is:
```ts 
interface IExaltation extends IRealmEyePlayerResponse {
    exaltations: {
        class: string;
        exaltationAmount: string;
        exaltationStats: {
            [s: string]: number;
            health: number;
            magic: number;
            attack: number;
            defense: number;
            speed: number;
            vitality: number;
            wisdom: number;
            dexterity: number;
        };
    }[];
}
```

## Examples
Here are some examples. 

### Case 1 
```
GET api/realmeye/player/exaltations?name=consolemc
```
Response:
```json
{"exaltations":[],"profileIsPrivate":false,"sectionIsPrivate":false,"name":"consolemc","resultCode":200}
```

### Case 2
``` 
GET api/realmeye/player/exaltations?name=deatttthhh&prettify
```
Response: 
```json 
{
  "exaltations": [
    {
      "class": "Archer",
      "exaltationAmount": 2,
      "exaltationStats": {
        "health": 0,
        "magic": 0,
        "attack": 0,
        "defense": 0,
        "speed": 0,
        "vitality": 1,
        "wisdom": 1,
        "dexterity": 0
      }
    },
    {
      "class": "Knight",
      "exaltationAmount": 1,
      "exaltationStats": {
        "health": 1,
        "magic": 0,
        "attack": 0,
        "defense": 0,
        "speed": 0,
        "vitality": 0,
        "wisdom": 0,
        "dexterity": 0
      }
    },
    // ... omitted some entries
    {
      "class": "Warrior",
      "exaltationAmount": 28,
      "exaltationStats": {
        "health": 4,
        "magic": 2,
        "attack": 3,
        "defense": 2,
        "speed": 3,
        "vitality": 4,
        "wisdom": 5,
        "dexterity": 5
      }
    },
    {
      "class": "Wizard",
      "exaltationAmount": 4,
      "exaltationStats": {
        "health": 0,
        "magic": 0,
        "attack": 0,
        "defense": 0,
        "speed": 0,
        "vitality": 2,
        "wisdom": 2,
        "dexterity": 0
      }
    }
  ],
  "profileIsPrivate": false,
  "sectionIsPrivate": false,
  "name": "deatttthhh",
  "resultCode": 200
}
```