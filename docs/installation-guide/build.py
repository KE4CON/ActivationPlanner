"""
Activation Planner — User Manual builder (Word / house pipeline).

Single edition. Chapters are validated JSON under ./chapters/*.json using the shared house block
schema (h1/h2/p/steps/bullets/callout/screenshot/table). This builder emits a Markdown source of
truth plus a styled .docx (navy+gold house style from style.py). Screenshots are kept as figure
placeholders so images can be inserted into the Word file by hand, last.

Run:  python build.py
Env:  MANUAL_OUT overrides the .docx output DIRECTORY (used when Word has the file locked).
"""
import os
import sys
import glob
import json
import re
import datetime

HERE = os.path.dirname(__file__)
sys.path.insert(0, HERE)  # local house style.py
import style as S  # noqa: E402

TODAY = datetime.date.today().strftime("%B %d, %Y")


def load_chapters():
    out = []
    for path in sorted(glob.glob(os.path.join(HERE, "chapters", "*.json"))):
        with open(path, encoding="utf-8") as f:
            raw = json.load(f)
        ch = raw.get("chapter", raw)
        out.append((int(raw.get("order", ch.get("order", 999))), ch))
    out.sort(key=lambda x: x[0])
    return out


# ---- Markdown emitter (source of truth) -----------------------------------
def _md_inline(text):
    return re.sub(r"__(.+?)__", r"*\1*", str(text))


def _md_blocks(blocks):
    lines = []
    for blk in blocks:
        if not isinstance(blk, dict):
            continue
        if "h1" in blk:
            lines.append(f"\n## {blk['h1']}\n")
        elif "h2" in blk:
            lines.append(f"\n### {blk['h2']}\n")
        elif "p" in blk:
            lines.append(_md_inline(blk["p"]) + "\n")
        elif "steps" in blk:
            lines += [f"{i}. {_md_inline(s)}" for i, s in enumerate(blk["steps"], 1)]
            lines.append("")
        elif "bullets" in blk:
            lines += [f"- {_md_inline(b)}" for b in blk["bullets"]]
            lines.append("")
        elif "callout" in blk:
            c = blk["callout"] if isinstance(blk["callout"], dict) else {}
            label = c.get("label", c.get("kind", "NOTE").upper())
            lines.append(f"> **{label}** — {_md_inline(c.get('text', ''))}\n")
        elif "screenshot" in blk:
            lines.append(f"> _[Figure: {blk['screenshot']}]_\n")
        elif "table" in blk:
            t = blk["table"] if isinstance(blk["table"], dict) else {}
            headers = [str(h) for h in t.get("headers", [])]
            rows = t.get("rows", [])
            if headers:
                lines.append("| " + " | ".join(headers) + " |")
                lines.append("| " + " | ".join(["---"] * len(headers)) + " |")
                for r in rows:
                    lines.append("| " + " | ".join(_md_inline(c) for c in r) + " |")
                lines.append("")
    return "\n".join(lines)


def to_markdown(chapters):
    parts = [
        "# Activation Planner — Installation Guide\n",
        "*Download, run one setup command, and go — in plain language.*\n",
        f"*Generated {TODAY} · Markdown is the living source of truth.*\n",
        "\n---\n",
    ]
    for number, (_o, ch) in enumerate(chapters, 1):
        parts.append(f"\n# {number}. {ch.get('title', 'Untitled')}\n")
        if ch.get("subtitle"):
            parts.append(f"*{_md_inline(ch['subtitle'])}*\n")
        parts.append(_md_blocks(ch.get("blocks", [])))
    return "\n".join(parts)


# ---- styled .docx ---------------------------------------------------------
def build_docx(chapters, out_dir):
    doc = S.new_document(
        header_title="Activation Planner — Installation Guide",
        header_sub="Install on Windows, macOS, Linux (incl. Raspberry Pi)",
        footer_left="Activation Planner  ·  Installation Guide  ·  KE4CON",
    )
    S.cover(
        doc,
        kicker="ACTIVATION PLANNER",
        big_title="Activation Planner",
        subtitle="Ham Radio Operating Planner",
        doc_kind="INSTALLATION GUIDE",
        version="v1.0",
        tagline="Get it running in minutes — download, run one setup command, answer a few prompts, and you're planning. Windows, macOS, and Linux (including Raspberry Pi).",
        author="James Rospopo  ·  KE4CON",
        date_str=TODAY,
    )
    S.section_title(doc, "Contents")
    S.toc(doc)
    for number, (_o, ch) in enumerate(chapters, 1):
        S.render_chapter(doc, ch, number)
    out = os.path.join(out_dir, "Activation_Planner_Installation_Guide.docx")
    doc.save(out)
    return out


def main():
    chapters = load_chapters()
    if not chapters:
        print("No chapters in ./chapters/*.json yet.")
        return

    out_dir = os.environ.get("MANUAL_OUT") or HERE
    os.makedirs(out_dir, exist_ok=True)

    md_path = os.path.join(HERE, "Activation_Planner_Installation_Guide.md")
    with open(md_path, "w", encoding="utf-8") as f:
        f.write(to_markdown(chapters))

    docx_path = build_docx(chapters, out_dir)
    print(f"OK — {len(chapters)} chapter(s)")
    print(f"   md:  {md_path}")
    print(f"   docx:{docx_path}")


if __name__ == "__main__":
    main()
