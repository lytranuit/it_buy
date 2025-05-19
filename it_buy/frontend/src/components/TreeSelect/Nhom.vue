<template>
  <TreeSelect :options="nhom" v-model="value" :multiple="multiple" :normalizer="normalizer" :append-to-body="true"
    zIndex="3000" :limit="0" :limitText="(count) => 'Lọc: ' + count + ' nhóm'" :disabled="disabled" />
</template>
<script setup>
// import the component
import { storeToRefs } from "pinia";
import { onMounted, computed, ref } from "vue";

import { useGeneral } from "../../stores/general";

const store = useGeneral();
const { nhom } = storeToRefs(store);
const props = defineProps({
  value: Array,
  disabled: { type: Boolean, default: false },
  multiple: { type: Boolean, default: true },
});
const emit = defineEmits(["update:value"]);
const value = computed({
  get() {
    return props.value;
  },
  set(value) {
    emit("update:value", value);
  },
});

const normalizer = (node) => {
  let data = {
    id: node.manhom,
    label: node.manhom + " - " + node.tennhom,
  };
  return data;
};
onMounted(() => {
  store.fetchNhom();
});
</script>