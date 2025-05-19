import { ref, computed } from "vue";
import { defineStore } from "pinia";
// import VueJwtDecode from "vue-jwt-decode";

import { useCookies } from "vue3-cookies";
import authApi from "../api/authApi";
import userApi from "../api/userApi";
import Api from "../api/Api";
export const useAuth = defineStore("auth", () => {
  const data = ref({});
  const users = ref([]);
  const roles = ref([]);
  const departments = ref([]);
  const userdepartments = ref([]);
  const isAuth = computed(() => {
    const { cookies } = useCookies();
    const Token = cookies.get("Auth-Token");
    return Token ? true : false;
  });
  const user = computed(() => {
    const cacheUser = localStorage.getItem("me") || "{}";
    return JSON.parse(cacheUser);
  });
  const list_users = computed(() => {
    // console.log(user);
    return user.value.list_users;
  });

  const is_MasterData = computed(() => {
    return in_groups(["MasterData"]);
  });
  const is_admin = computed(() => {
    return in_groups(["Administrator"]);
  });
  const is_manager = computed(() => {
    return in_groups(["Manager Task"]);
  });
  const is_KHSX = computed(() => {
    return in_groups(["KHSX"]);
  });
  const is_BOM = computed(() => {
    return in_groups(["Manager BOM"]);
  })
  const is_RD = computed(() => {
    return in_groups(["RD"]);
  })
  const is_Kho_VT = computed(() => {
    return in_groups(["Kho_VT"]);
  })
  const is_Cungung = computed(() => {
    return in_departments([14, 29, 30]);
  });
  const is_CungungGiantiep = computed(() => {
    return in_departments([14]);
  });
  const is_CungungNVL = computed(() => {
    return in_departments([29]);
  });
  const is_CungungHCTT = computed(() => {
    return in_departments([30]);
  });
  const is_Ketoan = computed(() => {
    return in_departments([11]);
  });
  const is_LeadQa = computed(() => {
    return in_departments([16]);
  });
  async function getUser() {
    const { cookies } = useCookies();
    const Token = cookies.get("Auth-Token");
    const cacheUser = localStorage.getItem("me");
    if (!cacheUser || JSON.parse(cacheUser).token != Token) {
      return authApi.TokenInfo(Token).then((response) => {
        // console.log(localStorage.getItem("me"));
        if (response.success) {
          localStorage.setItem("me", JSON.stringify(response));
        }
        return response;
      });
    }
    return JSON.parse(cacheUser);
  }
  async function logout() {
    localStorage.removeItem("me");
    // document.getElementById("logoutForm").submit();
    authApi.logout().then((res) => {
      location.href = "/Identity/Account/Login";
    });
  }
  async function fetchRoles() {
    if (roles.value.length) return;
    return userApi.roles().then((response) => {
      roles.value = response;
      return response;
    });
  }
  async function fetchDepartment() {
    if (departments.value.length) return;
    return userApi.departments().then((response) => {
      departments.value = response;
      return response;
    });
  }

  const fetchDepartments = () => {
    if (departments.value.length) return;
    authApi.Departments().then((res) => {
      departments.value = res;
    });
  };
  async function fetchUserDepartment() {
    if (userdepartments.value.length) return;
    return Api.departments().then((response) => {
      userdepartments.value = response;
      return response;
    });
  }
  async function fetchUsers() {
    if (users.value.length) return;
    return authApi.users().then((response) => {
      users.value = response;
      return response;
    });
  }
  async function fetchData(id) {
    return userApi.get(id).then((response) => {
      data.value = response;
      return response;
    });
  }
  function in_groups(groups) {
    let re = false;
    let user_roles = user.value.roles;
    if (user_roles) {
      for (let d of user_roles) {
        if (groups.indexOf(d) != -1) {
          re = true;
          break;
        }
      }
    }
    return re;
  }
  function in_departments(departments) {
    let re = false;
    let user_departments = user.value.departments;
    if (user_departments) {
      for (let d of user_departments) {
        if (departments.indexOf(d) != -1) {
          re = true;
          break;
        }
      }
    }
    return re;
  }
  return {
    data,
    roles,
    users,
    departments,
    userdepartments,
    isAuth,
    user,
    is_admin,
    is_Kho_VT,
    is_manager,
    is_MasterData,
    is_Cungung,
    is_CungungGiantiep,
    is_CungungNVL,
    is_Ketoan,
    is_CungungHCTT,
    is_LeadQa,
    is_KHSX,
    is_BOM,
    is_RD,
    list_users,
    getUser,
    logout,
    fetchRoles,
    fetchDepartment,
    fetchUserDepartment,
    fetchDepartments,
    fetchUsers,
    fetchData,
  };
});
