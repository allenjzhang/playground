// const fs = require('fs');
// const https = require('http');
  
// // URL
// const url = 'http://cinema.acs.its.nyu.edu:8000/status-json.xsl';
  
// https.get(url,(res) => {
//     // Data will be stored at this path
    
//     const path = `${__dirname}/data/datacapture.txt`; 
//     const filePath = fs.createWriteStream(path);
//     res.pipe(filePath);
//     filePath.on('finish',() => {
//         filePath.close();
//         console.log('Download Completed'); 
//     })
    
// })

import{fetch} from 'node-fetch';
let url = "http://cinema.acs.its.nyu.edu:8000/status-json.xsl";
let settings = {method : "Get"};
fetch(url, settings)
    .then(res => res.json())
    .then((json)=> {
        console.log(json);
    })