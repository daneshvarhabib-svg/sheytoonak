// Telegram Stars for «مدينة الفاجعة»: a Cloudflare Worker (free plan is enough).
// Secrets (set in the dashboard, never in the repository or a chat):
//   BOT_TOKEN    the token of @MadinatAlFajiaBot
//   HOOK_SECRET  any long random text of your own
// Routes:
//   GET  /invoice?sku=..&init=<Telegram.WebApp.initData>  -> {"link": "..."}   (the game calls this)
//   POST /hook                                           Telegram's webhook: says yes to the pre-checkout question
//   GET  /setup?key=<HOOK_SECRET>                        once: points the bot's webhook at this worker
// ⚠️ The table is the same as ShopCatalog (LiraPacks, Specials) with ShopCatalog.StarsFor: change both together.
const PACKS = {
  lira_200:  { stars: 50,   title: '٢٠٠ بِلّية' },
  lira_500:  { stars: 100,  title: '٥٠٠ بِلّية' },
  lira_1200: { stars: 200,  title: '١٢٠٠ بِلّية' },
  lira_2500: { stars: 400,  title: '٢٥٠٠ بِلّية' },
  lira_6500: { stars: 1000, title: '٦٥٠٠ بِلّية' },
  special_a: { stars: 500,  title: 'عرض خاص' },
  special_b: { stars: 250,  title: 'عرض خاص' },
};
const CORS = { 'Access-Control-Allow-Origin': '*', 'Content-Type': 'application/json' };
const enc = new TextEncoder();

async function hmac(key, text) {
  const k = await crypto.subtle.importKey('raw', key, { name: 'HMAC', hash: 'SHA-256' }, false, ['sign']);
  return new Uint8Array(await crypto.subtle.sign('HMAC', k, enc.encode(text)));
}

// Telegram's own check of initData: only a page really opened by Telegram for this bot passes.
async function userOf(init, token) {
  const p = new URLSearchParams(init);
  const hash = p.get('hash'); if (!hash) return null;
  p.delete('hash');
  const lines = [...p.entries()].map(([k, v]) => k + '=' + v).sort().join('\n');
  const secret = await hmac(enc.encode('WebAppData'), token);
  const sig = [...await hmac(secret, lines)].map(b => b.toString(16).padStart(2, '0')).join('');
  if (sig !== hash) return null;
  if (Date.now() / 1000 - Number(p.get('auth_date') || 0) > 86400) return null;
  try { return JSON.parse(p.get('user')).id; } catch (e) { return null; }
}

async function tg(env, method, body) {
  const r = await fetch('https://api.telegram.org/bot' + env.BOT_TOKEN + '/' + method,
    { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
  return r.json();
}

export default {
  async fetch(req, env) {
    const url = new URL(req.url);
    try {
      if (url.pathname === '/invoice') {
        const pack = PACKS[url.searchParams.get('sku')];
        const user = await userOf(url.searchParams.get('init') || '', env.BOT_TOKEN);
        if (!pack || !user) return new Response('{"error":"bad"}', { status: 400, headers: CORS });
        const j = await tg(env, 'createInvoiceLink', {
          title: pack.title, description: 'مدينة الفاجعة', currency: 'XTR',
          payload: url.searchParams.get('sku') + ':' + user + ':' + Date.now(),
          prices: [{ label: pack.title, amount: pack.stars }],
        });
        if (!j.ok) return new Response('{"error":"telegram"}', { status: 502, headers: CORS });
        return new Response(JSON.stringify({ link: j.result }), { headers: CORS });
      }
      if (url.pathname === '/hook' && req.method === 'POST') {
        if (req.headers.get('X-Telegram-Bot-Api-Secret-Token') !== env.HOOK_SECRET) return new Response('no', { status: 403 });
        const up = await req.json();
        if (up.pre_checkout_query) {
          const q = up.pre_checkout_query, sku = String(q.invoice_payload || '').split(':')[0], pack = PACKS[sku];
          const ok = !!pack && q.currency === 'XTR' && q.total_amount === pack.stars;
          await tg(env, 'answerPreCheckoutQuery', { pre_checkout_query_id: q.id, ok, error_message: ok ? undefined : 'غير متاح' });
        }
        return new Response('ok');
      }
      if (url.pathname === '/setup') {
        if (!env.HOOK_SECRET || url.searchParams.get('key') !== env.HOOK_SECRET) return new Response('no', { status: 403 });
        const j = await tg(env, 'setWebhook', { url: url.origin + '/hook', secret_token: env.HOOK_SECRET,
          allowed_updates: ['pre_checkout_query', 'message'] });
        return new Response(j.ok ? 'webhook set' : 'failed');
      }
    } catch (e) { return new Response('{"error":"server"}', { status: 500, headers: CORS }); }
    return new Response('ok');
  },
};
