import { ref, computed } from "vue";
import { defineStore } from "pinia";
import Api from "../api/Api";

export const useGeneral = defineStore("general", () => {
  const materials = ref([]);
  const supliers = ref([]);
  const fetchMaterials = () => {
    if (materials.value.length) return;
    return Api.materials().then((response) => {
      materials.value = response;
      return response;
    });
  };

  const changeMaterial = (mahh, row) => {
    var material = materials.value.find((item) => "m-" + item.id == mahh);
    if (material) {
      row.mahh = material.mahh;
      row.tenhh = material.tenhh;
      row.dvt = material.dvt;
    } else {
      row.mahh = "";
      row.tenhh = "";
      row.dvt = "";
    }
  };
  async function fetchNhacc() {
    if (supliers.value.length) return;
    return Api.nhacc().then((response) => {
      supliers.value = response;
      return response;
    });
  }
  return {
    materials,
    supliers,
    fetchMaterials,
    fetchNhacc,
    changeMaterial,
  };
});
