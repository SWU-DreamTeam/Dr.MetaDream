const BLOCKSDK = require('blocksdk-js');
const client = new BLOCKSDK("VUrU3PDgpZmOB7U2vlM6zQa5UaY1bVPVRDNFwh3i", "https://testnet-api.blocksdk.com");

const walletName = process.argv[2]; // Unity에서 전달된 이름을 명령어 인자로 받음

client.ethereum.CreateAddress({
    name: walletName
})
    .then(response => {
        console.log("Wallet created successfully:");
        console.log("Address:", response.address);
        console.log("Private Key:", response.privateKey); // 프라이빗 키 출력
    })
    .catch(err => {
        console.error("Error creating wallet:", err);
    });
