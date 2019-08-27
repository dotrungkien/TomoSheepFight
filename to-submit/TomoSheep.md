# Tomo Sheep (ETH Sheep Fight)

![](/Tomo-Sheep-Icon.png)

Version 1.0, published 20190926.

## Feature

- P2P game
- Build by Unity, with Photon networking & Netheum core.
- Blockchain tech with Tomochain.

## For end user: Play the game

Playing this game is so easy.

Firstly, just grab the apk file, install it into your android phone.

Because of PoC purpose only, we decided to use Tomochain testnet instead of mainnet.

Open the app, the first thing you see will be like this:

![](/Screenshots/Screenshot_0_firsttime.png)

![](/Screenshots/Screenshot_1_insufficient_balance.png)

At first, we will create an account for your, so you dont need to worried about the private key or any key at all. By clicking `COPY & GO TO FAUCET` we will be redirected to [Tomo Faucet](https://faucet.testnet.tomochain.com). Get 15 Tomo just enough to get started. Of course you can get as more as you want.

Now you have some Tomo in your account and are eligible to play this game. Hit `PLAY` and enjoy new game if there is a ready player in that, or wait for other one join your game. The bet to join game is `1 Tomo`, the winner will get all!

![](/Screenshots/Screenshot_2_lobby.png)

Wait for other play join:

![](/Screenshots/Screenshot_3_create_game.png)

In game, each sheep have its own weight and point if it can break the enemy barrier. The heavier, the the stronger, but also the less point gain.

So you should design you strategy very carefully to get the highest score in the fastest time.

Press `HELP` if you need more detail:

![](/Screenshots/Screenshot_4_help.png)

You can leave the game anytime you want. But you should not, because you deposited at game will be transfer to the oppponent. You will lost 1 Tomo.

![](/Screenshots/Screenshot_5_exit.png)

At end game, winner will receive all Tomo bet in game.

![](/Screenshots/Screenshot_6_win.png)

and loser lost all

![](/Screenshots/Screenshot_7_lose.png)

## For developer

There are serveral main components in this project:

- Contracts
- Unity game
- Photon network server
- Nethereum core to connect with Tomochain testnet

(we will update latest source code later)

## Known issues

Due of short duration of development in this hackathon, we've faced many trouble, and some even still exitst in latest build.

- the Photon network still not stable. Somtime we cannot match the opponent. The raw solution is just close (or uninstall) the app then open it again.
- Click actions sometimes delays. As the consequence of many asynchronous actions between Photon network and Blockchain, we still can not control those 100% and it lead to some unwanted delay effects in this game. We still not have any effective solution yet.
- other minor bugs...

## Next Plan

In future in next versions, we are going to fix all bugs and publish the game not only Android version but also iOS and other platforms version, too. And of course, support multichain like Tomochain mainnet, ETH mainnet, Ropsten, Loom, Rinkeby....

