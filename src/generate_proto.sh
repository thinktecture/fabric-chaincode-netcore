#!/usr/bin/env bash

# https://github.com/grpc/grpc/blob/master/examples/csharp/helloworld/generate_protos.bat

NUGET_PATH=$HOME/.nuget
TOOLS_PATH=$NUGET_PATH/packages/grpc.tools/2.24.0/tools/macosx_x64/
PROTO_PATH=$GOPATH/src/github.com/hyperledger/fabric/protos
TARGET_PATH=./protos

rm -rf $TARGET_PATH
mkdir $TARGET_PATH
mkdir $TARGET_PATH/common
mkdir $TARGET_PATH/token
mkdir $TARGET_PATH/msp
mkdir $TARGET_PATH/ledger
mkdir $TARGET_PATH/ledger/queryresult
mkdir $TARGET_PATH/peer

cp $PROTO_PATH/common/common.proto $TARGET_PATH/common/
cp $PROTO_PATH/token/expectations.proto $TARGET_PATH/token/
cp $PROTO_PATH/token/transaction.proto $TARGET_PATH/token/
cp $PROTO_PATH/msp/identities.proto $TARGET_PATH/msp/
cp $PROTO_PATH/ledger/queryresult/kv_query_result.proto $TARGET_PATH/ledger/queryresult/
cp $PROTO_PATH/peer/chaincode.proto $TARGET_PATH/peer/
cp $PROTO_PATH/peer/chaincode_event.proto $TARGET_PATH/peer/
cp $PROTO_PATH/peer/chaincode_shim.proto $TARGET_PATH/peer/
cp $PROTO_PATH/peer/proposal.proto $TARGET_PATH/peer/
cp $PROTO_PATH/peer/proposal_response.proto $TARGET_PATH/peer/

sed -i '' '23 i\
    option csharp_namespace = "Chaincode.NET.Protos";
' $TARGET_PATH/common/common.proto
sed -i '' '11 i\
    option csharp_namespace = "Chaincode.NET.Protos";
' $TARGET_PATH/token/expectations.proto
sed -i '' '11 i\
    option csharp_namespace = "Chaincode.NET.Protos";
' $TARGET_PATH/token/transaction.proto
sed -i '' '12 i\
    option csharp_namespace = "Chaincode.NET.Protos";
' $TARGET_PATH/msp/identities.proto
sed -i '' '23 i\
    option csharp_namespace = "Chaincode.NET.Protos";
' $TARGET_PATH/ledger/queryresult/kv_query_result.proto
sed -i '' '22 i\
    option csharp_namespace = "Chaincode.NET.Protos";
' $TARGET_PATH/peer/chaincode.proto
sed -i '' '21 i\
    option csharp_namespace = "Chaincode.NET.Protos";
' $TARGET_PATH/peer/chaincode_event.proto
sed -i '' '15 i\
    option csharp_namespace = "Chaincode.NET.Protos";
' $TARGET_PATH/peer/chaincode_shim.proto
sed -i '' '22 i\
    option csharp_namespace = "Chaincode.NET.Protos";
' $TARGET_PATH/peer/proposal.proto
sed -i '' '22 i\
    option csharp_namespace = "Chaincode.NET.Protos";
' $TARGET_PATH/peer/proposal_response.proto

$TOOLS_PATH/protoc \
	-I=./protos \
	-I=$GOPATH/src/github.com/protocolbuffers/protobuf/src \
	--csharp_out ./Chaincode.NET/Chaincode.NET.Protos \
	--grpc_out ./Chaincode.NET/Chaincode.NET.Protos \
	./protos/common/common.proto \
	./protos/token/expectations.proto \
	./protos/token/transaction.proto \
	./protos/msp/identities.proto \
	./protos/ledger/queryresult/kv_query_result.proto \
	./protos/peer/chaincode.proto \
	./protos/peer/chaincode_event.proto \
	./protos/peer/chaincode_shim.proto \
	./protos/peer/proposal.proto \
	./protos/peer/proposal_response.proto \
	--plugin="protoc-gen-grpc=$TOOLS_PATH/grpc_csharp_plugin"
