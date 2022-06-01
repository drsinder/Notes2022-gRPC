protoc --doc_out=. --doc_opt=html,grpc.html protos/*.proto
copy .\obj\Debug\net6.0\Protos\*.cs ..\Proto\Protos\
copy .\grpc.html E:\Projects\2022gRPC\Notes2022Docs\Solution\