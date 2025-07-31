print("Adding shard1ReplSet...");
sh.addShard("shard1ReplSet/shard1a:27017,shard1b:27017");
print("Shard 1 added successfully");

print("Adding shard2ReplSet...");
sh.addShard("shard2ReplSet/shard2a:27017,shard2b:27017");
print("Shard 2 added successfully");

const dbName = "ehr_db";
const collName = "Patients";
const ns = `${dbName}.${collName}`;

print(`Enabling sharding on database ${dbName}...`);
sh.enableSharding(dbName);

print(`Sharding collection ${ns} on { ClinicId: 1 }...`);
sh.shardCollection(ns, { ClinicId: 1 });

print("Sharding setup complete!");
