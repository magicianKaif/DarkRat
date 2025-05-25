
# DarkRAT

**DarkRAT** is a lightweight Remote Access Tool (RAT) built in C#.  
Designed for educational and research purposes, this project demonstrates how reverse shell connections can be initiated and controlled through basic TCP communication.

> 🔥 Author: [Magician Slime](https://t.me/magician_slime)

---

## 🚀 Features

- TCP reverse shell connection
- Command execution with output
- Change directory (`cd`)
- Automatic persistence via Windows Registry (Run key)
- Lightweight and minimal footprint

---

## ⚠️ Disclaimer

> **This tool is intended for educational and ethical testing purposes only.**  
> Misuse of this software can lead to legal consequences. The author is not responsible for any misuse or damage caused by this tool.

---

## 🛠️ Setup

1. Replace the IP and Port in `DarkRAT.cs`:
   ```csharp
   static string ip = "YOUR_IP_HERE";
   static int port = YOUR_PORT_HERE;
   ```
2. Compile the script using Visual Studio or `csc`:
   ```bash
   csc DarkRAT.cs
   ```

3. Set up a listener on your machine:
   ```bash
   nc -lvnp 4444
   ```

4. Execute the client on the target (test VM).

---

## 🧠 Educational Value

This project helps you understand:

- Persistent techniques (registry-based)
- TCP communication in C#
- Remote shell execution
- Building malware-like tools ethically for red teaming

---
---

## 📜 License

MIT License – Use at your own risk.
