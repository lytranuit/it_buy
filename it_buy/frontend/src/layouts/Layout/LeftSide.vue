<template>
  <div class="left-sidenav">
    <ul class="metismenu left-sidenav-menu">
      <li v-if="is_admin">
        <router-link class="nav-link" to="/admin">
          <i class="fas fa-chart-bar"></i>
          <span>Admin</span>
        </router-link>
      </li>
      <!-- <li>
        <router-link class="nav-link" to="/">
          <i class="fas fa-chart-bar"></i>
          <span>Tổng quan</span>
        </router-link>
      </li> -->
      <li>
        <router-link class="nav-link" to="/dutru">
          <i class="ti-briefcase"></i>
          <span>Dự trù</span>
        </router-link>
      </li>
      <li>
        <router-link class="nav-link" to="/hanghoa">
          <i class="fab fa-product-hunt"></i>
          <span>Tổng hợp hàng hóa</span>
        </router-link>
      </li>

      <li v-if="is_Cungung">
        <router-link class="nav-link" to="/muahang">
          <i class="fas fa-store"></i>
          <span>Đề nghị mua hàng</span>
        </router-link>
      </li>
      <li v-if="is_Ketoan">
        <router-link class="nav-link" to="/muahang/thanhtoan">
          <i class="fas fa-wallet"></i>
          <span>Đề nghị thanh toán</span>
        </router-link>
      </li>
      <!-- <li>
        <router-link class="nav-link" to="/danhgianhacungcap">
          <i class="fab fa-product-hunt"></i>

          <span>Đánh giá nguyên liệu</span>
        </router-link>
      </li> -->
      <li v-if="is_admin">
        <a href="javascript: void(0);">
          <i class="fas fa-wrench"></i>
          <span>{{ $t("system") }}</span>
          <span class="menu-arrow"><i class="mdi mdi-chevron-right"></i></span>
        </a>

        <ul class="nav-second-level" aria-expanded="false">
          <li class="nav-item">
            <router-link class="nav-link" to="/user"
              ><i class="ti-control-record"></i>{{ $t("user") }}</router-link
            >
          </li>
        </ul>
      </li>
      <li>
        <a href="javascript: void(0);">
          <i class="fas fa-database"></i>
          <span>Dữ liệu</span>
          <span class="menu-arrow"><i class="mdi mdi-chevron-right"></i></span>
        </a>

        <ul class="nav-second-level" aria-expanded="false">
          <!-- <li class="nav-item">
            <router-link class="nav-link" to="/materials"
              ><i class="ti-control-record"></i>Danh mục hàng hóa</router-link
            >
          </li> -->
          <li class="nav-item" v-if="is_Cungung">
            <router-link class="nav-link" to="/supplier"
              ><i class="ti-control-record"></i>Nhà cung cấp</router-link
            >
          </li>
          <li class="nav-item" v-if="is_Cungung">
            <router-link class="nav-link" to="/producer"
              ><i class="ti-control-record"></i>Nhà sản xuất</router-link
            >
          </li>
          <!-- <li class="nav-item" v-if="is_KHSX || is_RD || is_admin">
            <router-link class="nav-link" to="/product"
              ><i class="ti-control-record"></i>Danh mục sản phẩm
            </router-link>
          </li> -->

          <!-- <li class="nav-item" v-if="is_BOM || is_admin">
            <router-link class="nav-link" to="/bom"
              ><i class="ti-control-record"></i>BOM
            </router-link>
          </li> -->
        </ul>
      </li>
    </ul>

    <!-- <div class="button-col">
      <span class="btn-text">Thu gọn</span>
    </div> -->
  </div>
</template>
<script setup>
import { useAuth } from "../../stores/auth";
import { onMounted, ref } from "vue";
import { useLayout } from "../composables/layout";
import { storeToRefs } from "pinia";

const { initMetisMenu, initActiveMenu } = useLayout();
const store = useAuth();
const {
  is_admin,
  is_Cungung,
  is_Ketoan,
  is_CungungNVL,
  is_Qa,
  is_CungungGiantiep,
  is_CungungHCTT,
  is_KHSX,
  is_RD,
  is_BOM,
} = storeToRefs(store);
onMounted(() => {
  initMetisMenu();
  initActiveMenu();
});
</script>
