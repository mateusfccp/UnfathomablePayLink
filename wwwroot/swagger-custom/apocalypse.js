(function () {
  "use strict";

  const HORSEMEN = [
    {
      seal: "CONQUEST",
      name: "Leiner Garzón",
      role: "The White Horseman",
      lore: "Architect of the SQLite vaults and the first EF Core migrations, he forged unique business seals and raised the Swagger & OpenAPI altar with X-Api-Key security.",
      src: "/swagger-custom/images/conquest.jpg",
    },
    {
      seal: "WAR",
      name: "Joaquín Forni",
      role: "The Crimson Horseman",
      lore: "Forger of the service and repository legions, he shaped PaymentService, InvoiceService, and the data repositories—waging war on tangled layers until v1.final.final prevailed.",
      src: "/swagger-custom/images/war.jpg",
    },
    {
      seal: "FAMINE",
      name: "Mateus Cordeiro",
      role: "The Black Horseman",
      lore: "Keeper of the domain models, API-Key gatekeeper, and bringer of asynchronous payments, webhooks, and Render deployment—starving out unused endpoints until only lean, documented code remained.",
      src: "/swagger-custom/images/famine.jpg",
    },
    {
      seal: "Death",
      name: "Mathias Schneider",
      role: "The Pale Horseman",
      lore: "Harbinger of the REST controllers, DTOs, and HTTP test scrolls—who faced down compiler errors and merge conflicts to resurrect the build with the immortal cry: 'code now compiles yippee.'",
      src: "/swagger-custom/images/death.jpg",
    }
  ];

  function createEl(tag, className, text) {
    const el = document.createElement(tag);
    if (className) {
      el.className = className;
    }
    if (text !== undefined) {
      el.textContent = text;
    }
    return el;
  }

  function ensureLightbox() {
    let dialog = document.getElementById("apoc-lightbox");
    if (dialog) {
      return dialog;
    }

    dialog = createEl("dialog", "apoc-lightbox");
    dialog.id = "apoc-lightbox";
    dialog.setAttribute("closedby", "any");
    dialog.setAttribute("aria-labelledby", "apoc-lightbox-title");

    const frame = createEl("div", "apoc-lightbox-frame");

    const img = createEl("img", "apoc-lightbox-img");
    img.id = "apoc-lightbox-img";
    img.alt = "";

    const footer = createEl("div", "apoc-lightbox-footer");
    const textWrap = createEl("div");

    const title = createEl("h3", "apoc-lightbox-title");
    title.id = "apoc-lightbox-title";

    const desc = createEl("p", "apoc-lightbox-desc");
    desc.id = "apoc-lightbox-desc";

    textWrap.appendChild(title);
    textWrap.appendChild(desc);

    const closeForm = document.createElement("form");
    closeForm.method = "dialog";

    const closeBtn = createEl("button", "apoc-lightbox-close", "Close");
    closeBtn.type = "submit";
    closeForm.appendChild(closeBtn);

    footer.appendChild(textWrap);
    footer.appendChild(closeForm);

    frame.appendChild(img);
    frame.appendChild(footer);
    dialog.appendChild(frame);
    document.body.appendChild(dialog);

    // Mandatory fallback for browsers without native <dialog closedby="any"> support
    if (!("closedBy" in HTMLDialogElement.prototype)) {
      dialog.addEventListener("click", function (event) {
        if (event.target !== dialog) {
          return;
        }
        const rect = dialog.getBoundingClientRect();
        const isInsideDialog =
          rect.top <= event.clientY &&
          event.clientY <= rect.top + rect.height &&
          rect.left <= event.clientX &&
          event.clientX <= rect.left + rect.width;

        if (!isInsideDialog) {
          dialog.close();
        }
      });
    }

    return dialog;
  }

  function openLightbox(horseman) {
    const dialog = ensureLightbox();
    const img = document.getElementById("apoc-lightbox-img");
    const title = document.getElementById("apoc-lightbox-title");
    const desc = document.getElementById("apoc-lightbox-desc");

    img.src = horseman.src;
    img.alt = horseman.alt || (horseman.seal + " — " + horseman.name);
    title.textContent = horseman.seal + " — " + horseman.name;
    desc.textContent = horseman.lore;

    dialog.showModal();
  }

  function buildHeroSection() {
    const stage = createEl("section", "apoc-stage");
    stage.id = "apoc-stage";
    stage.setAttribute("aria-label", "Unfathomable PayLink banner");

    const hero = createEl("div", "apoc-hero");
    const heroImg = createEl("img", "apoc-hero-bg");
    heroImg.src = "/swagger-custom/images/banner.jpg";
    heroImg.alt = "Pixel art of the Four Horsemen of the Apocalypse riding across a bridge";

    const scrim = createEl("div", "apoc-hero-scrim");
    const heroContent = createEl("div", "apoc-hero-content");
    const heroLeft = createEl("div");

    const kicker = createEl("p", "apoc-kicker", "PAYMENT GATEWAY • APOCALYPSE EDITION");
    const title = createEl("h2", "apoc-hero-title", "UNFATHOMABLE PAYLINK");
    const subtitle = createEl(
      "p",
      "apoc-hero-subtitle",
      "Authorize your requests with X-Api-Key, process asynchronous payments, and receive webhook notifications when the seals are broken."
    );

    heroLeft.appendChild(kicker);
    heroLeft.appendChild(title);
    heroLeft.appendChild(subtitle);

    const badges = createEl("div", "apoc-badges");
    badges.appendChild(createEl("span", "apoc-badge", "X-Api-Key Auth"));
    badges.appendChild(createEl("span", "apoc-badge", "Webhooks JSON"));
    badges.appendChild(createEl("span", "apoc-badge", "OpenAPI 3.1"));

    heroContent.appendChild(heroLeft);
    heroContent.appendChild(badges);

    hero.appendChild(heroImg);
    hero.appendChild(scrim);
    hero.appendChild(heroContent);
    stage.appendChild(hero);
    return stage;
  }

  function buildTeamSection() {
    const team = createEl("section", "apoc-team");
    team.id = "apoc-team";
    team.setAttribute("aria-label", "Project Team — The Four Horsemen");

    const galleryHeader = createEl("div", "apoc-gallery-header");
    galleryHeader.appendChild(createEl("h3", "apoc-gallery-title", "The Four Horsemen — Project Team"));
    galleryHeader.appendChild(
      createEl("p", "apoc-gallery-hint", "Click any horseman to read their lore and enlarge the pixel art")
    );
    team.appendChild(galleryHeader);

    const grid = createEl("div", "apoc-horsemen-grid");
    HORSEMEN.forEach(function (horseman) {
      const card = createEl("button", "apoc-card");
      card.type = "button";
      card.setAttribute("aria-label", "Enlarge " + horseman.seal + ": " + horseman.name);

      const media = createEl("div", "apoc-card-media");
      const img = createEl("img", "apoc-card-img");
      img.src = horseman.src;
      img.alt = horseman.alt || (horseman.seal + " — " + horseman.name);
      img.loading = "lazy";

      const seal = createEl("span", "apoc-card-seal", horseman.seal);
      media.appendChild(img);
      media.appendChild(seal);

      const body = createEl("div", "apoc-card-body");
      body.appendChild(createEl("h4", "apoc-card-name", horseman.name));
      body.appendChild(createEl("p", "apoc-card-role", horseman.role));

      card.appendChild(media);
      card.appendChild(body);
      card.addEventListener("click", function () {
        openLightbox(horseman);
      });

      grid.appendChild(card);
    });

    team.appendChild(grid);
    return team;
  }

  function injectApocalypseTheme() {
    const swaggerUi = document.querySelector("#swagger-ui .swagger-ui");
    if (!swaggerUi) {
      return false;
    }

    if (!document.getElementById("apoc-stage")) {
      const topbar = swaggerUi.querySelector(":scope > .topbar");
      const stage = buildHeroSection();
      if (topbar) {
        topbar.insertAdjacentElement("afterend", stage);
      } else {
        swaggerUi.prepend(stage);
      }
    }

    if (!document.getElementById("apoc-team")) {
      const team = buildTeamSection();
      const existingFooter = document.getElementById("apoc-footer");
      if (existingFooter && existingFooter.parentNode === swaggerUi) {
        swaggerUi.insertBefore(team, existingFooter);
      } else {
        swaggerUi.appendChild(team);
      }
    }

    if (!document.getElementById("apoc-footer")) {
      const footer = createEl(
        "footer",
        "apoc-footer",
        "UNFATHOMABLE PAYLINK • FOUR HORSEMEN OF THE APOCALYPSE"
      );
      footer.id = "apoc-footer";
      swaggerUi.appendChild(footer);
    }

    ensureLightbox();
    return true;
  }

  function readAuthorizedApiKey() {
    try {
      if (
        window.ui &&
        window.ui.authSelectors &&
        typeof window.ui.authSelectors.authorized === "function"
      ) {
        const uiAuth = window.ui.authSelectors.authorized();
        if (uiAuth && typeof uiAuth.getIn === "function") {
          return uiAuth.getIn(["ApiKeyAuth", "value"]) || null;
        }
        return null;
      }

      const raw = window.localStorage.getItem("authorized");
      if (!raw) {
        return null;
      }

      const parsed = JSON.parse(raw);
      return parsed && parsed.ApiKeyAuth ? parsed.ApiKeyAuth.value || null : null;
    } catch (_) {
      return null;
    }
  }

  function hookAuthSpecRefresh() {
    if (window.__apocAuthRefreshHooked) {
      return;
    }
    window.__apocAuthRefreshHooked = true;

    let lastApiKey = readAuthorizedApiKey();
    let refreshTimer = null;

    function scheduleRefreshIfAuthChanged() {
      if (refreshTimer) {
        clearTimeout(refreshTimer);
      }
      refreshTimer = setTimeout(function () {
        refreshTimer = null;
        const nextApiKey = readAuthorizedApiKey();
        if (nextApiKey === lastApiKey) {
          return;
        }
        lastApiKey = nextApiKey;
        if (
          window.ui &&
          window.ui.specActions &&
          typeof window.ui.specActions.download === "function"
        ) {
          const specUrl =
            window.ui.specSelectors &&
            typeof window.ui.specSelectors.url === "function"
              ? window.ui.specSelectors.url() || "/openapi/v1.json"
              : "/openapi/v1.json";
          window.ui.specActions.download(specUrl);
        }
      }, 20);
    }

    const origSetItem = Storage.prototype.setItem;
    Storage.prototype.setItem = function (key) {
      origSetItem.apply(this, arguments);
      if (this === window.localStorage && key === "authorized") {
        scheduleRefreshIfAuthChanged();
      }
    };

    const origRemoveItem = Storage.prototype.removeItem;
    Storage.prototype.removeItem = function (key) {
      origRemoveItem.apply(this, arguments);
      if (this === window.localStorage && key === "authorized") {
        scheduleRefreshIfAuthChanged();
      }
    };

    function attachStoreSubscription() {
      if (
        window.ui &&
        typeof window.ui.getStore === "function" &&
        !window.__apocStoreSubscribed
      ) {
        window.__apocStoreSubscribed = true;
        lastApiKey = readAuthorizedApiKey();
        window.ui.getStore().subscribe(scheduleRefreshIfAuthChanged);
        return true;
      }
      return false;
    }

    if (!attachStoreSubscription()) {
      const waitForUiTimer = setInterval(function () {
        if (attachStoreSubscription()) {
          clearInterval(waitForUiTimer);
        }
      }, 50);
    }
  }

  function bootstrap() {
    hookAuthSpecRefresh();
    if (injectApocalypseTheme()) {
      return;
    }

    const root = document.getElementById("swagger-ui") || document.body;
    if (!root) {
      return;
    }

    const observer = new MutationObserver(function () {
      if (injectApocalypseTheme()) {
        observer.disconnect();
      }
    });

    observer.observe(root, { childList: true, subtree: true });
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", bootstrap);
  } else {
    bootstrap();
  }
})();
