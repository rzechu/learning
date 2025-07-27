// setup_sharding.js
// This script adds shards to the router and enables sharding for collections.

// Function to wait for a specific condition
function waitFor(condition, message, timeout = 30000) {
    var start = new Date().getTime();
    while (true) {
        if (new Date().getTime() - start > timeout) {
            throw new Error("Timeout waiting for: " + message);
        }
        try {
            if (condition()) {
                print("Condition met: " + message);
                break;
            }
        } catch (e) {
            // Ignore errors during condition check, useful for "server not ready"
            print("Waiting... " + message + " (" + e.message + ")");
        }
        sleep(1000); // Wait for 1 second
    }
}

// Wait for config servers and shard replica sets to be primary
waitFor(function () {
    var status = rs.status();
    return status.ok === 1 && status.members.some(m => m.stateStr === "PRIMARY");
}, "config replica set primary", 60000); // Wait up to 60 seconds for config primary

// Now, connect to the admin database to perform sharding operations
use admin;

// Add Shards to the cluster, waiting for them to be ready
print("Adding Shard 1 to the cluster...");
waitFor(function () {
    var res = sh.addShard("shard1ReplSet/shard1a:27017,shard1b:27017");
    return res.ok === 1;
}, "shard1ReplSet added", 60000);

print("Adding Shard 2 to the cluster...");
waitFor(function () {
    var res = sh.addShard("shard2ReplSet/shard2a:27017,shard2b:27017");
    return res.ok === 1;
}, "shard2ReplSet added", 60000);

// Enable sharding for the database
print("Enabling sharding for 'ehr_db' database...");
var enableShardRes = sh.enableSharding("ehr_db");
if (enableShardRes.ok === 1) {
    print("Sharding enabled for ehr_db.");
} else {
    printjson(enableShardRes);
}

// Ensure indexes and shard collections
use ehr_db;

print("Creating index and sharding 'patients' collection...");
db.patients.createIndex({ "ClinicId": 1 });
var shardPatientsRes = sh.shardCollection("ehr_db.patients", { "ClinicId": 1 });
if (shardPatientsRes.ok === 1) {
    print("Collection 'patients' sharded.");
} else {
    printjson(shardPatientsRes);
}

print("Creating index and sharding 'visits' collection...");
db.visits.createIndex({ "ClinicId": 1 });
var shardVisitsRes = sh.shardCollection("ehr_db.visits", { "ClinicId": 1 });
if (shardVisitsRes.ok === 1) {
    print("Collection 'visits' sharded.");
} else {
    printjson(shardVisitsRes);
}

print("MongoDB Cluster Sharding Setup Complete!");