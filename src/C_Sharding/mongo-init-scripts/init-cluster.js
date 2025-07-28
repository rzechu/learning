sh.addShard("shard1ReplSet/shard1a:27017,shard1b:27017");
sh.addShard("shard2ReplSet/shard2a:27017,shard2b:27017");

sh.enableSharding("testdb");
sh.shardCollection("testdb.testcoll", { _id: 1 });
