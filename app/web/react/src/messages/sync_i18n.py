#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
AI 驱动的国际化翻译脚本
========================
以 zh.json 为原本，使用 Anthropic Claude 自动将所有 key 翻译成目标语言。

使用方法：
    # 增量翻译所有语言（只翻译缺失的 key，保留现有翻译）
    python3 sync_i18n.py

    # 全量翻译所有语言（重新翻译所有 key，覆盖现有翻译）
    python3 sync_i18n.py --full

    # 只翻译指定语言
    python3 sync_i18n.py --lang ko,vi,th

    # 使用 OpenAI（默认 Anthropic）
    python3 sync_i18n.py --provider openai

    # 指定 Anthropic 模型
    python3 sync_i18n.py --model claude-3-5-sonnet-20241022

    # 指定 API Key（也可通过环境变量）
    python3 sync_i18n.py --api-key sk-ant-xxx

    # 增量翻译并显示详细输出
    python3 sync_i18n.py --verbose

环境变量：
    ANTHROPIC_API_KEY  Anthropic API Key（默认使用）
    OPENAI_API_KEY     OpenAI API Key（provider=openai 时使用）

支持的目标语言：
    en, zh-tw, es, id, ms, vi, th, ko, km, jp
"""

import json
import os
import copy
import argparse
import time
import sys
import shutil
from datetime import datetime
from typing import Dict, List, Optional, Tuple

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

# ============================================================
# 目标语言配置
# ============================================================
LANGUAGE_CONFIG: Dict[str, Dict[str, str]] = {
    'en.json':    {'name': 'English',                      'code': 'en'},
    'zh-tw.json': {'name': 'Traditional Chinese (Taiwan)', 'code': 'zh-TW'},
    'es.json':    {'name': 'Spanish',                      'code': 'es'},
    'id.json':    {'name': 'Indonesian',                   'code': 'id'},
    'ms.json':    {'name': 'Malay',                        'code': 'ms'},
    'vi.json':    {'name': 'Vietnamese',                   'code': 'vi'},
    'th.json':    {'name': 'Thai',                         'code': 'th'},
    'ko.json':    {'name': 'Korean',                       'code': 'ko'},
    'km.json':    {'name': 'Khmer (Cambodian)',             'code': 'km'},
    'jp.json':    {'name': 'Japanese',                     'code': 'ja'},
}

# ============================================================
# 翻译提示词
# ============================================================
TRANSLATE_PROMPT = """\
You are a professional UI/UX text translator specializing in financial trading platform interfaces.

Translate the following JSON object's VALUES from Simplified Chinese to {target_language}.

Rules:
1. ONLY translate the values; keep every key exactly as-is.
2. Preserve ALL placeholders in {{variable}} or {{variable}} format completely unchanged.
3. Preserve HTML tags, URLs, and purely numeric/code strings unchanged.
4. Use a formal, professional tone suitable for a financial trading platform.
5. Return ONLY a valid JSON object — no markdown fences, no explanations.

Input:
{input_json}"""


# ============================================================
# 工具函数
# ============================================================

def get_flat_dict(obj: dict, prefix: str = '') -> Dict[str, str]:
    """将嵌套字典展平为 {dot.path: value} 的字典"""
    result: Dict[str, str] = {}
    for k, v in obj.items():
        full_key = f'{prefix}.{k}' if prefix else k
        if isinstance(v, dict):
            result.update(get_flat_dict(v, full_key))
        else:
            result[full_key] = str(v)
    return result


def set_nested(obj: dict, key_path: str, value: str) -> None:
    """按点路径设置嵌套字典的值"""
    parts = key_path.split('.')
    curr = obj
    for p in parts[:-1]:
        if p not in curr:
            curr[p] = {}
        curr = curr[p]
    curr[parts[-1]] = value


def rebuild_structure(flat: Dict[str, str], structure: dict) -> dict:
    """以 zh.json 结构为骨架，将扁平字典重建为嵌套字典"""
    result = copy.deepcopy(structure)
    for key_path, value in flat.items():
        set_nested(result, key_path, value)
    return result


def backup_file(filepath: str) -> Optional[str]:
    """备份文件，返回备份路径；文件不存在时返回 None"""
    if not os.path.exists(filepath):
        return None
    ts = datetime.now().strftime('%Y%m%d_%H%M%S')
    backup_dir = os.path.join(BASE_DIR, '.i18n_backup')
    os.makedirs(backup_dir, exist_ok=True)
    backup_path = os.path.join(backup_dir, f'{os.path.basename(filepath)}.{ts}.bak')
    shutil.copy2(filepath, backup_path)
    return backup_path


def _cache_path(lang_file: str) -> str:
    """返回该语言的翻译缓存文件路径（用于断点续传）"""
    cache_dir = os.path.join(BASE_DIR, '.i18n_cache')
    os.makedirs(cache_dir, exist_ok=True)
    return os.path.join(cache_dir, f'{lang_file}.cache.json')


def load_cache(lang_file: str) -> Dict[str, str]:
    """读取断点续传缓存"""
    path = _cache_path(lang_file)
    if os.path.exists(path):
        try:
            with open(path, encoding='utf-8') as f:
                return json.load(f)
        except Exception:
            pass
    return {}


def save_cache(lang_file: str, data: Dict[str, str]) -> None:
    """保存断点续传缓存"""
    with open(_cache_path(lang_file), 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)


def clear_cache(lang_file: str) -> None:
    """清除断点续传缓存"""
    path = _cache_path(lang_file)
    if os.path.exists(path):
        os.remove(path)


# ============================================================
# AI 翻译（Anthropic）
# ============================================================

def _translate_anthropic(
    items: Dict[str, str],
    target_language: str,
    client,
    model: str,
) -> Dict[str, str]:
    input_json = json.dumps(items, ensure_ascii=False, indent=2)
    prompt = TRANSLATE_PROMPT.format(
        target_language=target_language,
        input_json=input_json,
    )
    response = client.messages.create(
        model=model,
        max_tokens=8192,
        messages=[{'role': 'user', 'content': prompt}],
        temperature=0.1,
    )
    text = response.content[0].text.strip()
    # 清理可能的 markdown 代码块
    if text.startswith('```'):
        text = text.split('\n', 1)[1]
        text = text.rsplit('```', 1)[0].strip()
    return json.loads(text)


# ============================================================
# AI 翻译（OpenAI）
# ============================================================

def _translate_openai(
    items: Dict[str, str],
    target_language: str,
    client,
    model: str,
) -> Dict[str, str]:
    input_json = json.dumps(items, ensure_ascii=False, indent=2)
    prompt = TRANSLATE_PROMPT.format(
        target_language=target_language,
        input_json=input_json,
    )
    response = client.chat.completions.create(
        model=model,
        messages=[{'role': 'user', 'content': prompt}],
        temperature=0.1,
        response_format={'type': 'json_object'},
    )
    return json.loads(response.choices[0].message.content)


# ============================================================
# 带重试的批量翻译
# ============================================================

def translate_batch(
    items: Dict[str, str],
    target_language: str,
    provider: str,
    client,
    model: str,
    max_retries: int = 3,
) -> Dict[str, str]:
    """批量翻译一组 key-value，失败自动重试"""
    last_error: Optional[Exception] = None
    for attempt in range(1, max_retries + 1):
        try:
            if provider == 'openai':
                return _translate_openai(items, target_language, client, model)
            else:
                return _translate_anthropic(items, target_language, client, model)
        except json.JSONDecodeError as e:
            last_error = e
            print(f' [JSON 解析错误, 重试 {attempt}/{max_retries}]', end='', flush=True)
        except Exception as e:
            last_error = e
            print(f' [错误: {type(e).__name__}, 重试 {attempt}/{max_retries}]', end='', flush=True)
        time.sleep(2 ** attempt)
    raise RuntimeError(f'翻译失败（{max_retries} 次重试后）: {last_error}')


# ============================================================
# 翻译单个语言文件
# ============================================================

def translate_language(
    lang_file: str,
    lang_config: Dict[str, str],
    zh_flat: Dict[str, str],
    zh_structure: dict,
    provider: str,
    client,
    model: str,
    full_mode: bool = False,
    batch_size: int = 60,
    backup: bool = True,
    verbose: bool = False,
) -> Tuple[int, int]:
    """
    翻译单个语言文件。
    返回 (成功翻译 key 数, 失败 key 数)。
    """
    filepath = os.path.join(BASE_DIR, lang_file)
    lang_name = lang_config['name']

    # 读取现有翻译
    existing_flat: Dict[str, str] = {}
    if os.path.exists(filepath):
        with open(filepath, encoding='utf-8') as f:
            existing_flat = get_flat_dict(json.load(f))

    # 确定需要翻译的 key
    if full_mode:
        keys_to_translate = list(zh_flat.keys())
        print(f'\n  [{lang_file}] 全量模式 — 共 {len(keys_to_translate)} 个 key')
    else:
        keys_to_translate = [k for k in zh_flat if k not in existing_flat]
        skipped = len(zh_flat) - len(keys_to_translate)
        print(
            f'\n  [{lang_file}] 增量模式 — '
            f'需翻译 {len(keys_to_translate)} 个 / 已有 {skipped} 个 / 共 {len(zh_flat)} 个'
        )

    if not keys_to_translate:
        print(f'  ✓ {lang_file} 无需更新')
        return 0, 0

    # 加载断点续传缓存（全量模式也支持续传）
    cache = load_cache(lang_file) if not full_mode else {}
    if full_mode:
        # 全量模式清空旧缓存，重新开始
        clear_cache(lang_file)

    # 过滤掉缓存中已翻译的 key
    remaining_keys = [k for k in keys_to_translate if k not in cache]
    if cache:
        print(f'    ↻ 从缓存恢复 {len(cache)} 个 key，剩余 {len(remaining_keys)} 个待翻译')

    # 备份原文件
    if backup and os.path.exists(filepath):
        bak = backup_file(filepath)
        if verbose and bak:
            print(f'    📦 已备份至 {bak}')

    # 批量翻译
    translated_flat: Dict[str, str] = dict(cache)
    failed_keys: List[str] = []
    total_batches = (len(remaining_keys) + batch_size - 1) // batch_size if remaining_keys else 0

    for i in range(0, len(remaining_keys), batch_size):
        batch_keys = remaining_keys[i: i + batch_size]
        batch_items = {k: zh_flat[k] for k in batch_keys}
        batch_num = i // batch_size + 1

        print(
            f'    批次 {batch_num:>3}/{total_batches}'
            f'（{len(batch_keys):>3} key）...',
            end='', flush=True,
        )
        try:
            result = translate_batch(batch_items, lang_name, provider, client, model)
            ok_count = 0
            for k in batch_keys:
                translated_val = result.get(k)
                if translated_val is not None:
                    translated_flat[k] = translated_val
                    ok_count += 1
                else:
                    # AI 未返回该 key，降级使用中文原文
                    translated_flat[k] = zh_flat[k]
                    failed_keys.append(k)
            print(f' ✓ ({ok_count}/{len(batch_keys)})')
        except Exception as e:
            print(f' ✗ ({e})')
            for k in batch_keys:
                translated_flat[k] = zh_flat[k]
                failed_keys.append(k)

        # 每批完成后更新缓存（断点续传保障）
        save_cache(lang_file, translated_flat)

        if i + batch_size < len(remaining_keys):
            time.sleep(0.5)

    # 合并：增量模式保留已有翻译
    if full_mode:
        final_flat = translated_flat
    else:
        final_flat = {**existing_flat, **translated_flat}

    # 按 zh.json 结构重建并写入
    output = rebuild_structure(final_flat, zh_structure)
    with open(filepath, 'w', encoding='utf-8') as f:
        json.dump(output, f, ensure_ascii=False, indent=2)
        f.write('\n')

    # 翻译完成后清除缓存
    clear_cache(lang_file)

    missing = len(set(zh_flat.keys()) - set(final_flat.keys()))
    fail_count = len(failed_keys)
    success_count = len(keys_to_translate) - fail_count

    if missing == 0 and fail_count == 0:
        print(f'  ✓ {lang_file} 翻译完成（新增 {len(keys_to_translate)} 个 key）')
    else:
        print(
            f'  △ {lang_file} 翻译完成'
            f'（成功 {success_count}，降级 {fail_count}，缺失 {missing}）'
        )
        if verbose and failed_keys:
            for k in failed_keys[:10]:
                print(f'      - {k}')
            if len(failed_keys) > 10:
                print(f'      ... 共 {len(failed_keys)} 个降级 key')

    return success_count, fail_count


# ============================================================
# 命令行入口
# ============================================================

def main() -> None:
    parser = argparse.ArgumentParser(
        description='AI 驱动的国际化翻译脚本 — 以 zh.json 为原本',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''\
示例：
  # 增量翻译所有语言（默认，Anthropic Claude）
  python3 sync_i18n.py

  # 全量翻译所有语言（覆盖现有翻译）
  python3 sync_i18n.py --full

  # 只增量翻译 Korean 和 Vietnamese
  python3 sync_i18n.py --lang ko,vi

  # 全量翻译繁体中文，使用高质量模型
  python3 sync_i18n.py --full --lang zh-tw --model claude-3-5-sonnet-20241022

  # 使用 OpenAI 翻译
  python3 sync_i18n.py --provider openai

  # 增量翻译并显示详细输出
  python3 sync_i18n.py --verbose

环境变量：
  ANTHROPIC_API_KEY  Anthropic API Key（默认）
  OPENAI_API_KEY     OpenAI API Key（provider=openai 时）
''',
    )
    parser.add_argument(
        '--full', action='store_true',
        help='全量替换：重新翻译所有 key（默认：增量，只翻译缺失的 key）',
    )
    parser.add_argument(
        '--lang', type=str, default=None,
        help='指定目标语言，逗号分隔，如: ko,vi,th（默认：翻译所有语言）',
    )
    parser.add_argument(
        '--provider', choices=['anthropic', 'openai'], default='anthropic',
        help='AI 服务提供商（默认: anthropic）',
    )
    parser.add_argument(
        '--model', type=str, default=None,
        help=(
            'AI 模型名称（默认: anthropic=claude-haiku-4-5，'
            'openai=gpt-4o-mini）'
        ),
    )
    parser.add_argument(
        '--api-key', type=str, default=None,
        help='API Key（也可通过环境变量 ANTHROPIC_API_KEY / OPENAI_API_KEY 设置）',
    )
    parser.add_argument(
        '--batch-size', type=int, default=60,
        help='每批翻译的 key 数量（默认: 60）',
    )
    parser.add_argument(
        '--no-backup', action='store_true',
        help='翻译前不备份原文件',
    )
    parser.add_argument(
        '--verbose', '-v', action='store_true',
        help='显示详细输出（包括降级 key 列表）',
    )

    args = parser.parse_args()

    # 确定默认模型
    if args.model:
        model = args.model
    elif args.provider == 'anthropic':
        model = 'claude-haiku-4-5'
    else:
        model = 'gpt-4o-mini'

    # 初始化 AI 客户端
    if args.provider == 'anthropic':
        try:
            import anthropic  # type: ignore
        except ImportError:
            print('✗ 请先安装 anthropic 库: pip install anthropic')
            sys.exit(1)
        api_key = args.api_key or os.environ.get('ANTHROPIC_API_KEY')
        if not api_key:
            print('✗ 请设置 ANTHROPIC_API_KEY 环境变量，或使用 --api-key 参数')
            sys.exit(1)
        client = anthropic.Anthropic(api_key=api_key)

    else:  # openai
        try:
            from openai import OpenAI  # type: ignore
        except ImportError:
            print('✗ 请先安装 openai 库: pip install openai')
            sys.exit(1)
        api_key = args.api_key or os.environ.get('OPENAI_API_KEY')
        if not api_key:
            print('✗ 请设置 OPENAI_API_KEY 环境变量，或使用 --api-key 参数')
            sys.exit(1)
        client = OpenAI(api_key=api_key)

    # 读取 zh.json
    zh_path = os.path.join(BASE_DIR, 'zh.json')
    if not os.path.exists(zh_path):
        print(f'✗ 找不到源文件: {zh_path}')
        sys.exit(1)
    with open(zh_path, encoding='utf-8') as f:
        zh_structure = json.load(f)
    zh_flat = get_flat_dict(zh_structure)

    print('=' * 60)
    print('  AI 国际化翻译脚本')
    print('=' * 60)
    print(f'  源语言    : zh.json（{len(zh_flat)} 个 key）')
    print(f'  模式      : {"全量替换" if args.full else "增量翻译（保留已有翻译）"}')
    print(f'  AI 提供商 : {args.provider}')
    print(f'  模型      : {model}')
    print(f'  批次大小  : {args.batch_size}')

    # 确定要处理的语言列表
    if args.lang:
        lang_files: List[str] = []
        for token in args.lang.split(','):
            token = token.strip()
            if not token.endswith('.json'):
                token += '.json'
            if token in LANGUAGE_CONFIG:
                lang_files.append(token)
            else:
                print(f'⚠  未知语言标识: {token}，已跳过')
    else:
        lang_files = list(LANGUAGE_CONFIG.keys())

    print(f'  目标语言  : {", ".join(lang_files)}')
    print('=' * 60)

    # 翻译每个语言，统计结果
    start_time = time.time()
    total_success = 0
    total_failed = 0
    errored_langs: List[str] = []

    for lang_file in lang_files:
        try:
            ok, fail = translate_language(
                lang_file=lang_file,
                lang_config=LANGUAGE_CONFIG[lang_file],
                zh_flat=zh_flat,
                zh_structure=zh_structure,
                provider=args.provider,
                client=client,
                model=model,
                full_mode=args.full,
                batch_size=args.batch_size,
                backup=not args.no_backup,
                verbose=args.verbose,
            )
            total_success += ok
            total_failed += fail
        except Exception as e:
            print(f'  ✗ {lang_file} 翻译失败: {e}')
            errored_langs.append(lang_file)

    elapsed = time.time() - start_time
    print()
    print('=' * 60)
    print(f'  翻译完成！耗时 {elapsed:.1f}s')
    print(f'  成功翻译 : {total_success} 个 key')
    if total_failed:
        print(f'  降级回退 : {total_failed} 个 key（已用中文原文填充）')
    if errored_langs:
        print(f'  失败语言 : {", ".join(errored_langs)}')
    print('=' * 60)


if __name__ == '__main__':
    main()
