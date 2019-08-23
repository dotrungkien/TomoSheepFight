const SheepFight = artifacts.require('SheepFight');
// const SheepFaucet = artifacts.require('SheepFaucet');

module.exports = function(deployer) {
  deployer.deploy(SheepFight);
  // deployer.deploy(SheepFaucet);
};
