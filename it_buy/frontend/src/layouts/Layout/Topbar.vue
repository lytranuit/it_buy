<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from "vue";
import { useLayout } from "../composables/layout";
import { useRouter } from "vue-router";
import { useAuth } from "../../stores/auth";
import TieredMenu from "primevue/tieredmenu";
const store = useAuth();
const user = store.user;
const { layoutConfig, onMenuToggle, contextPath, curentLang } = useLayout();

const outsideClickListener = ref(null);
const topbarMenuActive = ref(false);
const router = useRouter();

onMounted(() => {
  bindOutsideClickListener();
  // var is_first = localStorage.getItem("is_first") || 0;
  // if (!is_first) {
  //   localStorage.setItem("is_first", 1);
  //   router.push("/member/changepassword");
  // }
});

onBeforeUnmount(() => {
  unbindOutsideClickListener();
});

const logoUrl = computed(() => {
  return `${contextPath}layout/images/${layoutConfig.darkTheme.value ? "logo-white" : "logo-dark"
    }.svg`;
});

const onTopBarMenuButton = () => {
  menu1.value.toggle(event);
};
const topbarMenuClasses = computed(() => {
  return {
    "layout-topbar-menu-mobile-active": topbarMenuActive.value,
  };
});

const bindOutsideClickListener = () => {
  if (!outsideClickListener.value) {
    outsideClickListener.value = (event) => {
      if (isOutsideClicked(event)) {
        topbarMenuActive.value = false;
      }
    };
    document.addEventListener("click", outsideClickListener.value);
  }
};
const unbindOutsideClickListener = () => {
  if (outsideClickListener.value) {
    document.removeEventListener("click", outsideClickListener);
    outsideClickListener.value = null;
  }
};
const isOutsideClicked = (event) => {
  if (!topbarMenuActive.value) return;

  const sidebarEl = document.querySelector(".layout-topbar-menu");
  const topbarEl = document.querySelector(".layout-topbar-menu-button");

  return !(
    sidebarEl.isSameNode(event.target) ||
    sidebarEl.contains(event.target) ||
    topbarEl.isSameNode(event.target) ||
    topbarEl.contains(event.target)
  );
};
const menu = ref();
const menu1 = ref();
const items = ref([
  {
    label: "Thông tin tài khoản",
    icon: "pi pi-fw pi-user text-muted ",
    to: "/member",
  },
  {
    separator: true,
  },
  {
    label: "Đăng xuất",
    icon: "pi pi-fw pi-power-off text-muted",
    command: (event) => {
      // event.originalEvent: Browser event
      // event.item: Menuitem instance
      store.logout();
    },
  },
]);
const changeLanguage = (lang) => {
  localStorage.setItem("language", lang);
  location.reload();
};
const toggle = (event) => {
  menu.value.toggle(event);
};
const application = ref({
  "Esign": {
    "hide": false,
    "active": false,
    "link": "https://esign.astahealthcare.com/",
    "label": "Chữ ký điện tử",
    "image": "/images/esign.png",
    "width": 35
  },
  "Esign-gmp": {
    "hide": false,
    "active": false,
    "link": "https://esign-gmp.astahealthcare.com/",
    "label": "Hồ sơ GMP",
    "image": "/images/gmp.png",
    "width": 28
  },
  "Task": {
    "hide": false,
    "active": false,
    "link": "https://task.astahealthcare.com/",
    "label": "Công việc",
    "image": "/images/task.png",
    "width": 28
  },
  "Workflow": {
    "hide": false,
    "active": false,
    "link": "https://flow.astahealthcare.com/",
    "label": "Quy trình",
    "image": "/images/workflow.png",
    "width": 28
  },
  "QrCode": {
    "hide": false,
    "active": false,
    "link": "https://qrcode.astahealthcare.com/",
    "label": "QR Code",
    "image": "/images/qrcode.png",
    "width": 28
  },
  "Purchase": {
    "hide": false,
    "active": true,
    "link": "https://purchase.astahealthcare.com/",
    "label": "Mua hàng",
    "image": "/images/purchase.png",
    "width": 28
  }
})
</script>
<template>
  <div class="layout-topbar">
    <router-link to="/" class="layout-topbar-logo justify-content-center">
      <span>
        <img src="../../assets/images/clientlogo_astahealthcare.com_f1800.png" alt="logo-large"
          class="logo-lg logo-light" width="100" />
      </span>
    </router-link>

    <button class="p-link layout-menu-button layout-topbar-button" @click="onMenuToggle()">
      <i class="pi pi-bars"></i>
    </button>
    <div class="d-none d-xl-block h-100">
      <div class="soft" v-for="(item, key) in application" :key="key" v-show="!item.hide"
        :class="{ 'active': item.active }">
        <a :href="item.link">
          <img :src="item.image" :width="item.width" alt="" />
          <strong class="d-block">{{ item.label }}</strong>
        </a>
      </div>
    </div>
    <button class="p-link layout-topbar-menu-button layout-topbar-button" @click="onTopBarMenuButton()">
      <i class="pi pi-ellipsis-v"></i>
    </button>

    <TieredMenu id="overlay_tmenu" ref="menu1" :model="items" :popup="true" style="width: 200px"></TieredMenu>
    <div class="layout-topbar-menu" :class="topbarMenuClasses">
      <!-- <div class="dropdown">
        <a class="nav-link dropdown-toggle waves-effect waves-light" data-toggle="dropdown" href="javascript: void(0);"
          role="button" aria-haspopup="false" aria-expanded="false" v-if="curentLang == 'en'">
          English
          <img src="../../assets/images/us_flag.jpg" class="ml-2" height="16" alt="" />
          <i class="mdi mdi-chevron-down"></i>
        </a>
        <a class="nav-link dropdown-toggle waves-effect waves-light" data-toggle="dropdown" href="javascript: void(0);"
          role="button" aria-haspopup="false" aria-expanded="false" v-if="curentLang == 'vi'">
          Tiếng việt
          <img src="../../assets/images/vi_flag.png" class="ml-2" height="16" alt="" />
          <i class="mdi mdi-chevron-down"></i>
        </a>
        <div class="dropdown-menu dropdown-menu-right" x-placement="bottom-end" style="
            position: absolute;
            will-change: transform;
            top: 0px;
            left: 0px;
            transform: translate3d(1314px, 70px, 0px);
          ">
          <a class="dropdown-item" href="javascript: void(0);" @click="changeLanguage('en')"><span> English </span><img
              src="../../assets/images/us_flag.jpg" alt="" class="ml-2 float-right" height="14" /></a>
          <a class="dropdown-item" href="javascript: void(0);" @click="changeLanguage('vi')"><span> Tiếng Việt </span><img
              src="../../assets/images/vi_flag.png" alt="" class="ml-2 float-right" height="14" /></a>
        </div>
      </div> -->
      <button class="p-link" @click="toggle" aria-haspopup="true" aria-controls="overlay_tmenu">
        <img :src="user.image_url" alt="profile-user" class="rounded-circle" width="40" />
        {{ user.FullName }}
      </button>
      <TieredMenu id="overlay_tmenu" ref="menu" :model="items" :popup="true" style="width: 200px"></TieredMenu>
    </div>
  </div>
</template>
<style lang="scss" scoped>
.soft {
  height: 100%;
  align-content: center;
  justify-items: center;
  display: inline-grid;
  padding: 10px;
  text-align: center;
}

.soft.active {
  background: #ededed;
}
</style>