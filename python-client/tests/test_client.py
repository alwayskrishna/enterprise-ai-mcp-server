from pathlib import Path
import ast

def test_python_client_syntax():
    client = Path(__file__).resolve().parents[1] / "client.py"
    ast.parse(client.read_text(encoding="utf-8"))

def test_client_has_entrypoint():
    client = Path(__file__).resolve().parents[1] / "client.py"
    assert 'asyncio.run(main())' in client.read_text(encoding="utf-8")
