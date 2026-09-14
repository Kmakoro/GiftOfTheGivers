document.addEventListener('DOMContentLoaded', () => {
  const header = document.getElementById('siteHeader');
  const onScroll = () => header?.classList.toggle('scrolled', window.scrollY > 8);
  onScroll(); window.addEventListener('scroll', onScroll, { passive: true });

  const amountInput = document.getElementById('donationAmount');
  document.querySelectorAll('.amount-chip').forEach(chip => chip.addEventListener('click', () => {
    document.querySelectorAll('.amount-chip').forEach(c => c.classList.remove('active'));
    chip.classList.add('active');
    if (amountInput) { amountInput.value = chip.dataset.amount || ''; amountInput.focus(); }
  }));

  const currencyCode = document.getElementById('currencyCode');
  document.querySelectorAll('input[name="Input.Currency"]').forEach(input => {
    const sync = () => { if (currencyCode && input.checked) currencyCode.textContent = input.value; };
    input.addEventListener('change', sync); sync();
  });

  document.querySelectorAll('.password-toggle').forEach(btn => btn.addEventListener('click', () => {
    const input = btn.parentElement?.querySelector('input'); if (!input) return;
    input.type = input.type === 'password' ? 'text' : 'password';
    const icon = btn.querySelector('i'); if (icon) icon.className = input.type === 'password' ? 'bi bi-eye' : 'bi bi-eye-slash';
  }));

  document.querySelectorAll('[data-sidebar-toggle]').forEach(btn => btn.addEventListener('click', () => document.getElementById('employeeSidebar')?.classList.toggle('open')));
});
