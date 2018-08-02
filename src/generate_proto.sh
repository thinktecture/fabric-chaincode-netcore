#!/usr/bin/env bash

# https://github.com/grpc/grpc/blob/master/examples/csharp/helloworld/generate_protos.bat

NUGET_PATH=$HOME/.nuget
TOOLS_PATH=$NUGET_PATH/packages/grpc.tools/1.11.1/tools/macosx_x64/
PROTO_PATH=$GOPATH/src/github.com/hyperledger/fabric/protos
TARGET_PATH=./protos

rm -rf $TARGET_PATH
mkdir $TARGET_PATH
mkdir $TARGET_PATH/common
mkdir $TARGET_PATH/msp
mkdir $TARGET_PATH/ledger
mkdir $TARGET_PATH/ledger/queryresult
mkdir $TARGET_PATH/peer

cp $PROTO_PATH/common/common.proto $TARGET_PATH/common/
cp $PROTO_PATH/msp/identities.proto $TARGET_PATH/msp/
cp $PROTO_PATH/ledger/queryresult/kv_query_result.proto $TARGET_PATH/ledger/queryresult/
cp $PROTO_PATH/peer/chaincode.proto $TARGET_PATH/peer/
cp $PROTO_PATH/peer/chaincode_event.proto $TARGET_PATH/peer/
cp $PROTO_PATH/peer/chaincode_shim.proto $TARGET_PATH/peer/
cp $PROTO_PATH/peer/proposal.proto $TARGET_PATH/peer/
cp $PROTO_PATH/peer/proposal_response.proto $TARGET_PATH/peer/

$TOOLS_PATH/protoc \
	-I=./protos \
	-I=$GOPATH/src/github.com/gogo/protobuf/protobuf \
	--csharp_out ./Chaincode/Chaincode.Protos \
	--grpc_out ./Chaincode/Chaincode.Protos \
	./protos/common/common.proto \
	./protos/msp/identities.proto \
	./protos/ledger/queryresult/kv_query_result.proto \
	./protos/peer/chaincode.proto \
	./protos/peer/chaincode_event.proto \
	./protos/peer/chaincode_shim.proto \
	./protos/peer/proposal.proto \
	./protos/peer/proposal_response.proto \
	--plugin="protoc-gen-grpc=$TOOLS_PATH/grpc_csharp_plugin"
