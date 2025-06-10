const local = require('../system/localization');
const sql = require('mssql');
const option = require('../system/option.js');


exports.Query = async (req, res) => {
    const { SQLType, SQLquery } = req.body;
    console.log('✅', local.getLocalTimeNow(), ' : ', '[Query Reqeusted]\n' + SQLquery.toString().substring(0,300) + "...");
    if (!SQLquery) {
        return res.status(400).json({ error: "SQLquery is required" });
    }

    try {
        await sql.connect(option.dbConfig);
        const result = await sql.query(SQLquery);

        res.json({ "items": result.recordset });

        console.log('✅',  local.getLocalTimeNow(), ' : ', '[Query Result]\n' + JSON.stringify(result.recordset)); //.substring(0,300) + "..."
    }
    catch (err) {
        res.status(500).json({ error: err.message });
        console.log("name:", err.name);
        console.log("message:", err.message);
        console.log("stack:", err.stack);


        console.log('✅',  local.getLocalTimeNow(), ' : ',
            'Query Failed : \n' + SQLquery.toString().substring(50) + "...");
    }
}
