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