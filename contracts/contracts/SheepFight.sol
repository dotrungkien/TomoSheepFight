pragma solidity 0.5.0;

contract SheepFight {
    struct Game {
        string id;
        address leftPlayer;
        address secondPlayer;
    }

    Game[] public games;

    mapping (address => bool) public isPlaying;
    mapping (address => uint) public playerToGame;

    uint public betValue = 1 ether;

    function searchGame(string memory gameID)
        public
        returns (int)
    {
        for (uint i = 0; i < games.length; i++)
        {
            if (compareStringsbyBytes(games[i].id, gameID)) return int(i);
        }
        return -1;
    }

    function play(string calldata gameID)
        external
        payable
    {
        require(msg.value >= betValue, "must bet 1 value");
        require(!isPlaying[msg.sender], "player must not be in game");


        if (msg.value > betValue) {
            msg.sender.transfer(msg.value - betValue);
        }
        isPlaying[msg.sender] = true;
        playerToGame[msg.sender] = gameID;
    }

    function endGame(string calldata gameID, bool isWon)
        external
    {
        require(isPlaying[msg.sender], "player must be in game");
        if (isWon) {
            require(address(this).balance >= 2*betValue, "insufficient balance");
            msg.sender.transfer(2*betValue);
        }
        isPlaying[msg.sender] = false;
        playerToGame[msg.sender] = "";
    }

    function forceEndGame()
        external
    {
        isPlaying[msg.sender] = false;
        playerToGame[msg.sender] = "";
    }

    function compareStringsbyBytes(string memory s1, string memory  s2)
        public
        pure
        returns(bool)
    {
        return keccak256(abi.encodePacked(s1)) == keccak256(abi.encodePacked(s2));
    }

    function () external payable {}
}