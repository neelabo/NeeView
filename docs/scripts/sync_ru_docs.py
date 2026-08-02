import os
import glob
from bs4 import BeautifulSoup

def load_restext(path):
    d = {}
    with open(path, 'r', encoding='utf-8') as f:
        for line in f:
            line = line.strip()
            if '=' in line and not line.startswith('#'):
                key, val = line.split('=', 1)
                d[key.strip()] = val
    return d

def sync_docs():
    en_path = r'NeeView/Languages/en.restext'
    ru_path = r'NeeView/Languages/ru.restext'
    
    # Use absolute paths if run from root
    if not os.path.exists(en_path):
        en_path = os.path.join(os.path.dirname(__file__), '..', '..', en_path)
        ru_path = os.path.join(os.path.dirname(__file__), '..', '..', ru_path)

    if not os.path.exists(en_path) or not os.path.exists(ru_path):
        print(f"Error: Localization files not found.")
        return

    en_dict = load_restext(en_path)
    ru_dict = load_restext(ru_path)
    
    # Build translation map: EN text -> RU text
    # Only keep entries where both exist and they differ
    translations = {}
    for key, en_text in en_dict.items():
        if key in ru_dict:
            ru_text = ru_dict[key]
            if en_text and ru_text and en_text != ru_text:
                # To prevent overriding with empty or same strings
                translations[en_text] = ru_text
                
    # Add manual missing headers if they are not in restext but are in UI
    manual_overrides = {
        'Argument': 'Аргумент',
        'Summary': 'Описание',
        'Name': 'Имя',
        'Type': 'Тип',
        'Bookmark': 'Закладка',
        'Effect': 'Эффект',
        'Fields': 'Поля'
    }
    translations.update(manual_overrides)
    
    # We will only apply these to ru-ru docs
    docs_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'ru-ru')
    html_files = glob.glob(os.path.join(docs_dir, '*.html'))
    
    for html_file in html_files:
        print(f"Processing {os.path.basename(html_file)}...")
        with open(html_file, 'r', encoding='utf-8') as f:
            content = f.read()
            
        parts = content.split('---', 2)
        if len(parts) == 3:
            frontmatter = f'---{parts[1]}---'
            body = parts[2]
        else:
            frontmatter = ''
            body = content

        soup = BeautifulSoup(body, 'html.parser')

        # To avoid replacing inside english anchor links or class names,
        # we only replace text nodes.
        for element in soup.find_all(string=True):
            parent = element.parent
            if parent.name in ['script', 'style', 'code', 'pre', 'a']: 
                # keep <a> tags mostly untouched unless they perfectly match, but to be safe, avoid 'a' if it contains API names.
                # Actually, let's process <a> if it's an exact match
                if parent.name in ['script', 'style', 'code', 'pre']:
                    continue
            
            text = str(element)
            original = text
            stripped = text.strip()
            
            if not stripped:
                continue
                
            # 1. Exact match strategy (Best for short UI words like 'Prev', 'Next', 'Name')
            if stripped in translations:
                text = text.replace(stripped, translations[stripped])
            else:
                # 2. Substring strategy (Only for longer sentences to avoid breaking words like 'Book' in 'BookAccessor')
                for en_text, ru_text in translations.items():
                    if len(en_text) > 15 and ' ' in en_text: 
                        if en_text in text:
                            text = text.replace(en_text, ru_text)

            if text != original:
                element.replace_with(text)

        translated_body = str(soup)
        
        with open(html_file, 'w', encoding='utf-8') as f:
            f.write(frontmatter + translated_body)
            
    print("Done synchronizing documentation.")

if __name__ == '__main__':
    sync_docs()
