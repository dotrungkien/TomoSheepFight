pragma solidity 0.5.0;

contract SheepFight {
    struct Battle {
        string id;
        address leftPlayer;
        address rightPlayer;
        uint result; // 0 - playing, 1 - leftwin, 2 - rightwin
    }

    mapping (address => bool) public isPlaying;

    uint public betValue = 1 ether;
    address[] public waitlist;
    Battle[] public battles;

    event StartBattle(address indexed leftPlayer, address indexed rightPlayer);

    modifier onlyReady() {
        require(!isPlaying[msg.sender], "player must not be in game");
        _;
    }

    modifier onlyPlaying() {
        require(isPlaying[msg.sender], "player must be in game");
        _;
    }

    function play()
        external
        payable
        onlyReady
    {
        require(msg.value >= betValue, "must bet 1 value");
        if (msg.value > betValue) {
            msg.sender.transfer(msg.value - betValue);
        }
        isPlaying[msg.sender] = true;
    }

    function endGame(bool isWon)
        external
        onlyPlaying
    {
        if (isWon) {
            require(address(this).balance >= 2*betValue, "insufficient balance");
            msg.sender.transfer(2*betValue);
        }
        isPlaying[msg.sender] = false;
    }

    function () external payable {}
}