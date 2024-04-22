<template>
  <div class="row clearfix">
    <div class="col-12">
      <form method="POST" id="form">
        <section class="card card-fluid">
          <div class="card-body">
            <Form></Form>
          </div>
        </section>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { onMounted, computed } from "vue";
import { useAuth } from "../../stores/auth";

import Steps from 'primevue/steps';
import Button from "primevue/button";
import Calendar from "primevue/calendar";
import FormMuahangChitiet from '../../components/Datatable/FormMuahangChitiet.vue'
import AlertError from "../../components/AlertError.vue";
import { useRoute, useRouter } from "vue-router";
import muahangApi from "../../api/muahangApi";
import { useMuahang } from '../../stores/muahang';
import { storeToRefs } from "pinia";
import { useToast } from "primevue/usetoast";
import Form from "../../components/muahang/Form.vue";
const toast = useToast();
const route = useRoute();
const storeMuahang = useMuahang();
const router = useRouter();
const messageError = ref();
const store = useAuth();
const { model, datatable, list_add } = storeToRefs(storeMuahang);
onMounted(() => {
  // console.log(datatable.value)
  model.value = {};
  if (!datatable.value.length) {
    router.push("/muahang");
  }
  model.value.type_id = list_add.value[0].type_id;
  model.value.status_id = 1;
  model.value.activeStep = 2;
});
</script>
