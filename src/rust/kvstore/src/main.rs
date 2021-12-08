use std::fs;

fn main() {
    let mut arguments = std::env::args().skip(1);
    let key = arguments.next().unwrap();
    let value = arguments.next().unwrap();
    print!("{} {}", key, value);
    match writedb(key, value)
    {
        Ok(()) => { print!("All good")}
        Err(error) => { print!("Got error {}", error)}
    }
}

fn writedb(key: String, value: String) -> Result<(), std::io::Error>
{
    return fs::write("kv.db", format!("{} {}", key, value));
}
