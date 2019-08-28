# Tomo Sheep Fight

![](https://serving.photos.photobox.com/22295168cd6268e8798ee938583d34748655c89111d60e106613b6c07b7cbd38a0e949db.jpg)

Version 1.0, published 20190926.
Author: [Do Trung Kien](https://github.com/dotrungkien)

## Feature

- P2P game
- Build by Unity, with Photon network & Netheum core.
- Blockchain tech with Tomochain.

## For end user: Play game

Playing this game is so easy.

Firstly, just grab the apk file, install it into your android phone.

Because of PoC purpose only, we decided to use Tomochain testnet instead of mainnet.

Open the app, the first thing you see will be like this:

![](https://serving.photos.photobox.com/0716930051370bf186dec758cf213c5fb223a91cce6af1b942f186d0cd1ca27c5e00fb4e.jpg)

![](https://serving.photos.photobox.com/250859539a8b021c97d6ad50bbcdd6642ed57acd0fe1b0f53a59775e66c9825ab746f492.jpg)

At first, we create an account for your, so you don't need to worry about the private key or any key at all. 

By clicking `COPY & GO TO FAUCET` we will be redirected to [Tomo Faucet](https://faucet.testnet.tomochain.com). Get 15 Tomo just enough to get started. Of course you can get as much as you want.

Now you have some Tomo in your account and are eligible to play this game. Hit `PLAY` and enjoy new game if there is a ready player in that, or wait for another one join your game. The bet to join game is 1 Tomo, the winner will get all!

![](https://serving.photos.photobox.com/5474546104b8a0ac45afe5efe17f03788714d77bef437e955799d9e2a985f3839f16b2c9.jpg)

Wait for other play join:

![](https://serving.photos.photobox.com/24386528c7606cb4308084ee8d2b48872d9203082ee4d45d6759a7fd690692344a1b9d10.jpg)

In the game, each sheep has its own weight and point if it can break the enemy barrier. The heavier, stronger, but also the less point gain.

So you should design you strategy very carefully to get the highest score in the fastest time.

Press `HELP` if you need more detail:

![](https://serving.photos.photobox.com/26997761f3059a639326011ed4419a534927fe1024cf91ca64b512cfa6f918c4ed9ebdc0.jpg)

You can leave the game anytime you want. But you should not, because your deposit in game will be transfer to the opponent. You will lose 1 Tomo!!

![](https://serving.photos.photobox.com/879304775d3fb09a5ca4a9fe3ac9e5f83b3d5a7d1a2af8f5e4ef5f9e7ec4febbb9460e4b.jpg)

At end game, winner will receive all Tomo bet in game.

![](https://serving.photos.photobox.com/753154118b919940817ffbc64311b1a6d004b6d7a8fda1bf4f570b540e8e7183225990a3.jpg)

and loser lost all

![](https://serving.photos.photobox.com/313969016ed7a3c128cfa35553b0958997b9d6e48269f76cc08b8f984108614381c0f82d.jpg)

## For developer

There are several main components in this project:

- Contracts - created by Truffle
- Game - created by Unity
- Realtime multiplayer server - with Photon network
- Tomochain testnet connector - with Nethereum core

### Contract

We created contract with `Truffle 5` and Solidity `0.5.0`. Currently, for fast prototype, we skip all cheat verifycation steps, and focus on play logic only.

```js
pragma solidity 0.5.0;

contract SheepFight {
    struct Game {
        string id;
        address payable leftPlayer;
        address payable rightPlayer;
        uint wonID; // 0 NA, 1 left win, 2 rightwin
    }

    Game[] public games;

    mapping (address => bool) public isPlaying;
    mapping (address => uint) public playerToGame;

    uint public betValue = 1 ether;

    constructor () public {
        games.push(Game("123456", address(0), address(0), 0));
    }

    function searchGame(string memory gameID)
        internal
        view
        returns (uint)
    {
        for (uint i = 0; i < games.length; i++)
        {
            if (compareStringsbyBytes(games[i].id, gameID)) return i;
        }
        return 0;
    }

    function compareStringsbyBytes(string memory s1, string memory  s2)
        public
        pure
        returns(bool)
    {
        return keccak256(abi.encodePacked(s1)) == keccak256(abi.encodePacked(s2));
    }

    function play(string calldata gameID)
        external
        payable
    {
        require(msg.value >= betValue, "must bet 1 value");
        require(!isPlaying[msg.sender], "player must not be in game");

        isPlaying[msg.sender] = true;
        uint gameIdx = searchGame(gameID);
        if (gameIdx == 0) {
            createGame(gameID);
        } else {
            joinGame(gameIdx);
        }
        if (msg.value > betValue) msg.sender.transfer(msg.value - betValue);
    }

    function createGame(string memory gameID)
        internal
        returns (uint)
    {
        Game memory newGame = Game(gameID, msg.sender, address(0), 0);
        uint latestGame = games.push(newGame) - 1;
        playerToGame[msg.sender] = latestGame;
        return latestGame;
    }

    function joinGame(uint gameIdx)
        internal
    {
        Game storage game = games[gameIdx];
        game.rightPlayer = msg.sender;
        playerToGame[msg.sender] = gameIdx;
    }


    function winGame()
        external
    {
        require(isPlaying[msg.sender], "player must be in game");
        uint gameIdx = playerToGame[msg.sender];
        require(gameIdx != 0, "not exist game");
        Game storage game = games[gameIdx];
        require(game.wonID == 0, "game was ended");
        game.wonID = (msg.sender == game.leftPlayer) ? 1 : 2;
        reward(msg.sender);
        resetPlayer();
    }

    function loseGame()
        external
    {
        if (isPlaying[msg.sender]) {
            isPlaying[msg.sender] = false;
        }
        uint gameIdx = playerToGame[msg.sender];
        if (gameIdx != 0) {
            playerToGame[msg.sender] = 0;
        }
    }

    function forceEndGame()
        external
    {
        uint gameIdx = playerToGame[msg.sender];
        if (gameIdx == 0) return;
        Game storage game = games[gameIdx];
        if (game.leftPlayer == msg.sender && game.rightPlayer != address(0)) reward(game.rightPlayer);
        if (game.rightPlayer == msg.sender && game.leftPlayer != address(0)) reward(game.leftPlayer);
        resetPlayer();
    }

    function reward(address payable to)
        public
        payable
    {
        require(address(this).balance >= 2*betValue, "insufficient balance");
        to.transfer(2*betValue);
    }

    function resetPlayer()
        internal
    {
        uint gameIdx = playerToGame[msg.sender];
        if (gameIdx == 0) return;
        isPlaying[msg.sender] = false;
        playerToGame[msg.sender] = 0;
        Game storage game = games[gameIdx];
        address leftPlayer = game.leftPlayer;
        if (leftPlayer != address(0)) {
            isPlaying[leftPlayer] = false;
            playerToGame[leftPlayer] = 0;
        }
        address rightPlayer = game.rightPlayer;
        if (rightPlayer != address(0)) {
            isPlaying[rightPlayer] = false;
            playerToGame[rightPlayer] = 0;
        }
    }

    function () external payable {}
}
```

In next step, we will record every step that user player sent to both the contract and photon server. Thus, we can verify the match result and prevent player from cheating.

### Unity game

There 2 scene in this game:

1. Lobby

At lobby we init the Photon network, Sheep Fight smart contract, set up player and match making. 

Please check the Unity project `Scenes/Lobby` Scene and `Scripts/Lobby/` for more detail.
  
2. Game

In game, there a `Game Controller` to control the sheep spawn, for both local player (base on player click) and remote player (base on Photon RPC call).

Please check the Unity project `Scenes/Game` and `Scripts/Game/` for more detail.

Through all, we keep two singleton in this scene and in all game, `GameManager` to keep all game information and `SheepContract` to interact with the smart contract.

In next step, sound & music also gonna be added.

### Photon network

- We use `PUN RPC` to communicate betweeen clients, and `MonobehaviorPUNCallbacks` for handle every network event.

### Smart contract interaction

- We use Nethereum to implement Web3 & contract instance in game. All transaction will be done asynchronously.


## Known issues

Due to short duration of development in this hackathon, we've faced many troubles, and some even still exist in the latest build.

As the consequence of many asynchronous actions between Game, Photon network and Blockchain, we still can not control those 100% and it lead to some unwanted delay effects in this game. We are trying to solve those.

## Next Plan

In next versions, we are going to fix all bugs and publish the game not only Android version but also iOS and other platforms version, too. And of course, support multichain like Tomochain mainnet, ETH mainnet, Ropsten, Loom, Rinkeby....

Enjoy gaming.

*Do Trung Kien
trungkien.keio@gmail.com*

